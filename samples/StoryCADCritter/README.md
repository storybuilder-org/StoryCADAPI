# StoryCAD Critter

A desktop app that uses an LLM to walk a StoryCAD outline element by element and produce a developmental critique — strengths, concerns, and questions for the author — for each Character, Setting, Problem, Scene, and the Story Overview.

The critique persona is *critic + coach + advocate*: it points at what's working as well as what's a gap, grounds findings in the Key Questions StoryCAD already uses for each element type, and stays on the author's side.

---

## What it does

1. Opens a `.stbx` outline you point it at.
2. For each critique-able element (Story Overview, Problem, Character, Setting, Scene), sends the element body plus relevant cross-references plus the type's Key Questions to the LLM.
3. Parses each response into `{ strengths, concerns, questionsForAuthor }`.
4. Writes three files into the output folder you pick:
   - `<name>.critique.md` — human-readable Markdown report
   - `<name>.costs.json` — token usage + USD cost for the run
   - `<name>.raw.json` — raw per-element LLM responses (useful when something looks off in the report)

If an outline is too thin to critique meaningfully (no Scenes, or fewer than 3 elements beyond the Overview), Critter short-circuits without making LLM calls and writes a structural-completeness note instead.

---

## Requirements

- **`OPENAI_API_KEY`** environment variable. Get one at <https://platform.openai.com/api-keys>. Set it before launching the app; the app reads it at run time.
- **`OPENAI_MODEL`** environment variable (optional). Defaults to `gpt-4o-mini`. Anything OpenAI's Chat Completions API accepts will work — `gpt-4o`, `gpt-4-turbo`, etc.
- An internet connection.
- A `.stbx` outline produced by StoryCAD 4.x.

The app is built against:

- **Windows**: WindowsAppSDK / WinUI 3, `net10.0-windows10.0.22621`
- **Mac / Linux / Windows (Skia)**: `net10.0-desktop` via Uno Platform

---

## How to run

1. Set `OPENAI_API_KEY` in your shell or system environment.
2. Launch `StoryCADCritter.exe`.
3. **Pick Outline (.stbx)** — choose the outline file you want critiqued.
4. **Pick Output Folder** — choose where the report and companion files will land.
5. **Run Critique** — the progress bar advances as each element completes; the log shows what's happening.
6. **Open Report** when done. **Cancel** at any time during a run stops cleanly.

---

## Output files

### `<name>.critique.md`

Markdown report with one section per element. Each section has:

- Element type + name as the heading
- `Strengths` — what's working, paired with the Key Question that surfaced it
- `Concerns` — what's missing, weak, or contradictory, paired with the Key Question
- `Questions for the Author` — open prompts the LLM thinks would sharpen the outline

When the LLM returns something that doesn't parse as the expected schema (truncated JSON, free-form prose), the section instead shows the raw text under a **"Couldn't parse the LLM response into the expected schema — raw text below"** banner. The run still completes.

### `<name>.costs.json`

```json
{
  "ModelId": "gpt-4o-mini",
  "LlmCallCount": 14,
  "InputTokens": 21450,
  "OutputTokens": 6100,
  "InputCostUsd": 0.00322,
  "OutputCostUsd": 0.00366
}
```

### `<name>.raw.json`

Every per-element LLM response in full, before Markdown rendering. Read this when the report shows a parse fallback and you want to see exactly what the model said, or when you're recording fixtures for tests.

---

## Performance, timing, and parallelism

The walk is **linear in element count** — one LLM call per critique-able element. There's no shortcut around that.

Per-call latency on `gpt-4o-mini` is roughly 5–15 seconds for a few-thousand-token prompt. Prompt size varies by element type (Problem calls include the full bodies of the Protagonist and Antagonist; Scene calls include the full Setting plus 1-line cast refs; Character/Setting calls use 1-line cross-refs only), so per-call time varies too.

Critter runs up to **4 elements in parallel** by default via a `SemaphoreSlim` cap. The cap is currently a hardcoded constant; making it configurable is a planned follow-up.

### Wall-clock estimates by outline size

Assuming ~10 s per call on `gpt-4o-mini`, no rate-limit throttling:

| Elements | Serial (≈1 at a time) | 4 in parallel (today) | 8 in parallel | 16 in parallel |
| ---: | ---: | ---: | ---: | ---: |
| 10 | ~100 s | ~30 s | ~20 s | ~10 s |
| 30 | ~5 min | ~80 s | ~40 s | ~20 s |
| 60 | ~10 min | ~2.5 min | ~80 s | ~40 s |
| 120 (novel-scale) | ~20 min | ~5 min | ~2.5 min | ~80 s |

### The other ceiling: OpenAI rate limits

OpenAI enforces per-account **tokens per minute (TPM)** and **requests per minute (RPM)** caps. For `gpt-4o-mini` the rough defaults are:

| Tier | TPM | RPM |
| --- | ---: | ---: |
| Free | 60 K | 3 |
| Tier 1 | 200 K | 500 |
| Tier 2 | 2 M | 5 000 |
| Tier 3+ | 4 M – 150 M | 5 000 – 30 000 |

A 120-element novel-scale run is roughly 700–900 K tokens total. On Tier 1, the **TPM floor** caps the run at about 3.5–4 minutes regardless of how many slots Critter uses in parallel — slots will sit waiting for the rate-limit window. On Tier 2+ this stops mattering at our volumes.

### What to do when it feels slow

- It almost certainly *isn't* hung. The log keeps writing as elements complete; if the last line was N/M complete and N hasn't moved, the next batch is mid-call. The 45 s per-call timeout (see Limitations) is the longest a single call can stall the walk.
- **Cancel** is always responsive.
- If you're consistently slow, check whether you're TPM-limited (above) — that's an OpenAI account issue, not a Critter issue.

### Cost is invariant in parallelism

You pay per token, not per second. 4-way and 16-way parallel runs cost the same. Use the largest parallelism your account tier supports without 429s.

---

## Known limitations

- **`MaxConcurrency = 4`** is hardcoded. Configurable concurrency is a planned follow-up.
- **Per-call timeout is 45 s**, retried 3× with 2 / 4 / 8 s exponential backoff. Worst case for one stuck element is ~150 s before it's marked failed and the walk continues.
- **No preferences UI yet.** Settings live in environment variables (`OPENAI_API_KEY`, `OPENAI_MODEL`) or in source.
- **The Key Questions used per element appear inline** in each element's report section. They're identical across same-type elements, which is repetitive. Planned follow-up: move to a once-per-report section gated by a preferences toggle.
- **Pre-existing outlines from older Outliner builds carry canonical placeholder UUIDs** (`11111111-1111-…`, `22222222-2222-…`, etc.). New Outliner runs produce real GUIDs. Critter prints whatever the `.stbx` contains; this isn't a Critter bug, but the report will look odd if you feed it an outline from a pre-fix Outliner run.

---

## Troubleshooting

**"OPENAI_API_KEY is not set."** — Set the environment variable in your shell *before* launching the app. Restart the app after setting it; environment variables aren't picked up after launch.

**The progress bar appears stuck for a long time.** — A single LLM call is probably slow. The 45 s per-call timeout × up to 3 retries means a single call can sit for ~150 s before being marked failed. If you genuinely need to bail out, click **Cancel**.

**A run completed but several elements show "Couldn't parse cleanly".** — The LLM returned non-JSON or truncated JSON for those elements. The raw text is in the report and in `.raw.json`. Re-running often clears it (LLM output is non-deterministic).

**Many elements fail with "rate limit" or 429-shaped errors.** — Your OpenAI account is hitting TPM/RPM caps. The orchestrator retries with backoff, but persistent caps mean you need a higher OpenAI tier or a lower concurrency. Wait a few minutes and re-run, or upgrade tier.

**The report's UUIDs look like `11111111-1111-…`.** — That `.stbx` was produced by an older Outliner build that didn't regenerate placeholder UUIDs. Generate a fresh outline from the current Outliner; Critter has no way to retro-fix existing files.
