# Issue #14 — StoryCADCritter harden + rebuild — status log

Sub-issue of storybuilder-org/StoryCAD#1246. Parallel to #13 (Outliner).
Branch: `issue-14-critter-rebuild` (not yet pushed/merged).

## Sessions

### 2026-05-18 — Per-element walk + companion files

**Done**
- `8eb766b` — replaced the rubric-based scoring critique with a per-element Key Questions walk: one LLM call per element (StoryOverview / Problem / Character / Setting / Scene), critic+coach+advocate persona externalized to `Prompts/CritiquePrompt.md`, output schema `{ strengths, concerns, questionsForAuthor }`, two-tier context granularity, minimal-outline short-circuit, defensive JSON parsing with raw-text fallback, exit-code mapping (0/1/2). `CritiqueOrchestrator` constructor-injects `StoryCADApi` + `IChatCompletionService` (no `Ioc.Default` inside).
- `7d3b33c` — orchestrator emits `<input>.costs.json` (token usage + USD) and `<input>.raw.json` (per-element raw responses) companion files, mirroring Outliner.

**Worked / didn't**
- Built and ran as a console app.

**Remains**
- Hardening, tests, README, UI (entered next session).

### 2026-05-20 → 2026-05-22 — Uno port, parallel walk, reliability, tests, README

**Done**
- Rebased the branch onto `main` (Jake's `7879370` "Various outliner tweaks + Fix ARM64"); 2 critter commits replayed clean.
- Baseline verification: Outliner + OutlinerTests build clean, OutlinerTests 59/59.
- `9420214` — converted Critter from console app to Uno.Sdk multi-target desktop app. TFMs `net10.0-desktop` + `net10.0-windows10.0.22621`. Single-page UX (pick `.stbx`, pick output folder, Run/Cancel, status line, determinate ProgressBar, log, Open Report/Folder). `RunAsync` signature changed to `IProgress<CritiqueProgress>` + optional `outputDirectory`. Parallel element walk: `SemaphoreSlim(MaxConcurrency=4)` + `Task.WhenAll`, serial body pre-fetch, `ConcurrentDictionary` cache, deterministic post-`WhenAll` ordering. Per-element 45s timeout via linked `CancellationTokenSource.CancelAfter`. Cancel button. Deleted console `Program.cs`.
- `0b0d370` (#13, on this branch) — `ProseAnalyzer.EnsureValidGuids` in Outliner now unconditionally regenerates element GUIDs via `Guid.NewGuid()` instead of only on `Guid.TryParse` failure. Cause: LLM emits valid-but-canonical placeholder GUIDs (`11111111-…`, confirmed in a live `.raw.json`) which the old logic accepted; OutlineBuilder passed them through to `AddElement` as the element UUID. OutlinerTests 59/59 still pass.
- `0931740` — new `StoryCADCritterTests` project (Microsoft.NET.Sdk, compile-includes Critter sources rather than cross-SDK ProjectReference). Three tests: stubbed happy path, malformed-response raw-text fallback, short-circuit on thin outline. 3/3 pass.
- `b988de4` — rewrote `samples/StoryCADCritter/README.md` as user-facing help: requirements, how-to-run, output files, performance/timing/parallelism (wall-clock table by outline size, OpenAI TPM/RPM tiers), known limitations, troubleshooting.
- `36876fc` — retry-visibility progress messages (`'X' timed out (attempt 2/3), retrying in 4s...`); completion message now carries a Stopwatch elapsed time and branches on complete / parse-fallback / FAILED; explicit OCE re-throw around backoff `Task.Delay`; `DefaultItemExcludes` extended with `StoryCADCritterTests\**`.
- Auto-scrolling log added (`ScrollViewer.ChangeView` on `TextChanged`); progress bar made visible (was `Height=6`).
- `fdf6272` / `364bb46` — `GetBody` change from a wrong diagnosis, reverted, plus a regression test. Detail under Worked/didn't.
- Issue #14 body updated on GitHub: 5 Hardening ACs and the Code-tasks Implementation checkbox flipped to `[x]`; added Status / upstream-fix / open-follow-ups sections.

**Worked / didn't**
- Both Critter TFMs build clean (3 warnings, all NU1903 Tmds.DBus.Protocol transitive). Critter tests 4/4.
- Live run reported as "hung": traced to silent 141s worst case per stuck element in the retry loop (45+2+45+4+45) holding semaphore slots; retry-visibility messages address the symptom.
- A separate "hung" report was the log scrollbar hiding completed content, not a hang — fixed by auto-scroll.
- Live run surfaced ungrounded critique (wrong pronoun, invented framing). I mis-diagnosed it as `GetBody` sending a bare UUID and changed it to `JsonSerializer.Serialize` (`9d98b4b`). That was wrong: `StoryCADApi.GetElement` returns `OperationResult<object>` whose `Payload` is already a JSON string (`element.Serialize()`), so the original `Payload.ToString()` was returning real data. The change double-encoded the body; reverted in `fdf6272`. The ungrounded critique is therefore NOT a `GetBody` bug — its real cause is still open.
- Added a prompt-content regression test (`364bb46`) asserting the serialized `ProtGoal` field name and value reach the prompt — guards against both a bare-UUID and a double-encoded body. The earlier stubbed tests only checked LLM responses, not what the prompt sends.

**Remains**
- Decide commit/untangle of the #13 fix riding on the #14 branch (user declined untangling).

**New tasks / issues**
- Preferences UI is the agreed next iteration: `MaxConcurrency` knob (currently a const, requested at 8), Key Questions placement toggle, likely a model picker (parallels Jake's Outliner Settings).
- Key Questions block: keep but move to a separate report gated by the preferences toggle (still rendered inline today).
- Open diagnostic: whether the SK OpenAI connector honors the per-call `CancellationToken` during the response-body read; the completion Stopwatch time will indicate it.
- Unverified: whether long-text fields (Notes/Description) come through as RTF in the serialized body.

### 2026-05-22 — Ungrounded critique root cause found and fixed

**Done**
- `933fe75` — added instruction to `CritiquePrompt.md` Mandate section: when referencing characters, use their specific attributes (name, sex, role) from cross-reference data rather than generic role labels ("protagonist"/"antagonist"). Verified with a live run against "The Long Ride Home": "Accident on the road" now names Joseph and Arrogance throughout; "Desire to attend the fair" names Becky and Joseph with correct pronouns.

**Worked / didn't**
- Root cause of ungrounded critique confirmed: `BuildCrossReferences` was already injecting full character bodies for Problem protagonist/antagonist GUIDs — the data was reaching the LLM. The issue was purely a prompt instruction gap; the model defaulted to generic role labels. One sentence in the Mandate fixed it.
- Used the StoryCAD MCP to open and inspect "The Long Ride Home.stbx" directly, then ran Critter twice (before and after the fix) for side-by-side comparison.

**Remains**
- ~~Key Questions rubric text uses "him" as a generic pronoun regardless of character sex.~~ Resolved 2026-05-23 (see below).

### 2026-05-22 — Bump parallel concurrency to 8

**Done**
- `MaxConcurrency` const raised 4 → 8 in `CritiqueOrchestrator.cs:49`. The progress message ("Critiquing N elements (up to {MaxConcurrency} in parallel)...") interpolates the const, so both the semaphore width and the displayed message now read 8. Addresses the "requested at 8" note under New tasks (2026-05-20 entry).

**Remains**
- Preferences UI knob for MaxConcurrency still open; this only changes the default const.

### 2026-05-23 — Key Questions rubric personalization (Minerva)

**Done**
- `CritiqueOrchestrator.PersonalizeKeyQuestions` — per-element copy of the shared per-type rubric with generic role nouns replaced by real names and generic pronouns replaced by the referent's actual gendered pronoun. Resolves the "him/her regardless of sex" item from the 2026-05-22 entry.
  - Character element: "the/your/this character" → element name; pronouns → the character's gendered pronoun (from the `Sex` field).
  - Problem element: "protagonist"/"antagonist" → the resolved Protagonist/Antagonist names; pronouns substituted only when a question references exactly one role (ambiguous both/neither left alone).
  - Sex blank (e.g. Arrogance, the Driver's Companion) → substitute the name (possessive → "Name's"); never guesses gender.
  - "her" object-vs-possessive disambiguated by following-word context, no POS parser.
  - Personalized list feeds both the LLM prompt and the rendered report; shared `_keyQuestionsByType` is never mutated.
- Two deterministic unit tests (male → masculine pronouns + name; unknown sex → name fallback). 6/6 Critter tests pass.

**Worked / didn't**
- Verified live against "The Long Ride Home" (stubbed walk, real `GetKeyQuestions` + real outline): Joseph's section now reads "he/him/his" (0 feminine pronouns); the "go to the fair" Problem reads "matters more to her" (Becky) vs "matters more to him" (Joseph); Arrogance falls back to its name.
- Environment note (Minerva): the WinUI samples must be built with VS MSBuild (`-p:Platform=x64`), not `dotnet build` (missing MSIX/PRI packaging task); GUI apps must be launched non-elevated or WinRT file pickers fail with `0x80004005`.

**Remains**
- None for the pronoun item.

## Carry-forward for #14 completion

Open acceptance criteria (issue #14 body):
- [ ] Unit tests for response-parsing logic (good + malformed variants). Partially covered by `StubbedWalk_MalformedResponse_FallsBackToRawText`.
- [ ] Integration test against a fixture with a mock `IChatCompletionService`. Covered by `StubbedWalk_LighthouseKeeper_ProducesReport`.
- [ ] Contract test pinning the per-element response schema.
- [ ] Tests run in CI on push to `main`/`dev`. Repo has only `deploy-docs.yml`; needs a decision with Jake.
- [x] README rewrite (`b988de4`).
- [ ] Pro-perk monetization README section — blocked on the #15 monetization decision.
- [ ] Code-reviewer pass.
- [ ] Human final approval.

Related follow-ups:
- [ ] Preferences UI iteration (MaxConcurrency, Key Questions placement, model picker).
- [x] #13 GUID fix stays on `issue-14-critter-rebuild` (pushed as `0b0d370`); merging this branch lands it. No separate branch/PR.
