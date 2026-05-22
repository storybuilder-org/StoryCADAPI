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
- `GetBody` serialization fix — replaced `Payload.ToString()` (returns UUID only) with `JsonSerializer.Serialize` on the runtime type. **Uncommitted on the branch as of this writing.**
- Issue #14 body updated on GitHub: 5 Hardening ACs and the Code-tasks Implementation checkbox flipped to `[x]`; added Status / upstream-fix / open-follow-ups sections.

**Worked / didn't**
- Both Critter TFMs build clean (3 warnings, all NU1903 Tmds.DBus.Protocol transitive). Critter tests 3/3.
- Live run reported as "hung": traced to silent 141s worst case per stuck element in the retry loop (45+2+45+4+45) holding semaphore slots; retry-visibility messages address the symptom.
- A separate "hung" report was the log scrollbar hiding completed content, not a hang — fixed by auto-scroll.
- Live run surfaced ungrounded critique (wrong pronoun, invented framing). Root cause: `StoryElement.ToString()` returns only the UUID, so `GetBody` had been sending the LLM a bare UUID as "element data" for every element, and `ExtractGuidsFromJson` found no cross-refs — the two-tier granularity never fired. Fixed by the `GetBody` serialization change (uncommitted).
- The stubbed tests verify LLM responses, not what the prompt sends — they did not catch the `GetBody` bug.

**Remains**
- Verify the `GetBody` fix on a live run (critique grounds in real `ConflictType` / `Sex` / goals); then commit it.
- Decide commit/untangle of the #13 fix riding on the #14 branch (user declined untangling).

**New tasks / issues**
- Preferences UI is the agreed next iteration: `MaxConcurrency` knob (currently a const, requested at 8), Key Questions placement toggle, likely a model picker (parallels Jake's Outliner Settings).
- Key Questions block: keep but move to a separate report gated by the preferences toggle (still rendered inline today).
- Open diagnostic: whether the SK OpenAI connector honors the per-call `CancellationToken` during the response-body read; the completion Stopwatch time will indicate it.
- Possible regression test gap: nothing asserts what the prompt sends to the LLM.
- Unverified: whether long-text fields (Notes/Description) come through as RTF in the serialized body.

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
- [ ] Verify + commit the `GetBody` serialization fix.
- [ ] Preferences UI iteration (MaxConcurrency, Key Questions placement, model picker).
- [ ] Decide whether the #13 GUID fix should land on its own branch/PR.
