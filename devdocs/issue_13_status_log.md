# Issue #13 — Outliner sample hardening — status log

Sub-issue of storybuilder-org/StoryCAD#1246.
Branch: `issue-13-outliner-hardening` — merged to `main` via PR #19 at `a94474c`, then deleted from local + origin.

## Sessions

### 2026-05-05 — Branch + build diagnosis
- Merged `issue-1246-repo-reorg` into `main` at `3c4d35d` and deleted; created `issue-13-outliner-hardening` from it, carrying Outliner WIP forward.
- Diagnosed VS NU1105 build error as cross-SDK `ProjectReference` between Outliner (`Microsoft.NET.Sdk`) and StoryCADLib (`Uno.Sdk`), compounded by StoryCADLib not being in `Outliner.sln`.

### 2026-05-06 — Build fix, test infra, first end-to-end run, prompt v2
- Added StoryCADLib to `samples/Outliner/Outliner.sln`. Solution now builds in VS without a CLI restore dance.
- Fixed stale-file pollution in `bin/.../TestInputs/`: `App.xaml.cs` now resolves `InputDir`/`OutputDir` to the source folders via a `FindProjectDir()` walk-up from `AppContext.BaseDirectory`; `OutlinerTests.csproj` no longer copies fixtures; `TestSetup` clears `*.stbx` at assembly init. Convention: `TestInputs/` processed, `TestStories/` held aside, `TestOutputs/` cleared each run.
- First CLI run: 29/30; the one failure was OpenAI 429 quota. After top-up: 30/30. Mirror, Mirror.docx produced a 4728-byte `.stbx` in 9s.
- Diagnosed three root causes for empty/dropped fields:
  1. `OnePassResponse.cs` had snake_case `[JsonPropertyName]` on inner fields; the LLM emits camelCase. Removed inner attributes; case-insensitive matching now resolves correctly.
  2. LLMs emit GUID-shaped strings with non-hex characters; `Guid.Parse` rejects silently. `ProseAnalyzer.ValidateGuids` now uses `TryParse`, regenerates invalid GUIDs, and remaps every cross-reference.
  3. `OutlineBuilder` property dicts used names absent from the StoryCAD models (`CharacterSketch`, `Relationships`, `ProtGoal`, `Cast`, `StoryQuestion`, `Significance`, `Title`). `UpdateElementProperties` short-circuits on the first bad key. Builders now use valid field names; non-mapping data folds into `Notes`; `CastMembers` populated via `AddCollectionEntry`.
- Prompt v2: rewrote `OnePassSystemPrompt.md`, bumped `OutlinePrompt.Version` to "v2". Schema added `StoryType`, `StoryGenre`, `StoryProblem`, `Concept` on `StoryOverviewElement`; `OutlineBuilder` maps them through.

### 2026-05-06 → 2026-05-07 — NavigationView shell, batch mode, settings, MVVM tests
- `9aa287a` — extracted `OutlineRunner` so single-mode UI, batch-mode UI, and tests share one pipeline.
- `66909a7` — `OutlinerPreferences` + `PreferencesService` persisted to `%LocalAppData%/Outliner/preferences.json`; registered in DI.
- `b406521` — replaced `MainWindow` content with a `NavigationView` shell + `Frame`.
- `19a64a0` — `BatchPage` + `BatchPageViewModel`: folder picker, per-file progress, continues on errors, defaults output to `<input>/Outlines/`.
- `215c1d9` — `SettingsPage` + `SettingsPageViewModel` (footer entry).
- `cb68055` — replaced stubbed pipeline tests with VM-targeted tests (`ContentPageViewModelTests`, `BatchPageViewModelTests`, `SettingsPageViewModelTests`). VMs no longer reference `DispatcherQueue`; tests touch only public surface (no `InternalsVisibleTo`).
- `5d81d57` — BatchPage "Pick Output Folder…" button overrides default `<input>/Outlines/`. SettingsPage added `DefaultInputFolder` / `DefaultOutputFolder` shared by both modes. Preferences schema simplified from four per-mode folders to two shared defaults plus `LastBatchInputFolder` / `LastBatchOutputFolder`.
- `985ddae` — `Story Content` textbox rebound from `ContentText` (status accumulator) to `StoryText` (the prose); status messages moved to a separate gray TextBlock. Tests now write to `OutlinerTests/TestOutputs/` so they don't pollute the user-facing sandbox. MainWindow nav switched from `SelectionChanged` to `ItemInvoked`.

### 2026-05-07 — Prompt v3 and IP cleanup
- `0149e47` — prompt v3. Every Problem now emits `subject`, `theme`, `method`, and all six goal/motive/conflict slots (`protGoal`/`protMotive`/`protConflict` + `antagGoal`/`antagMotive`/`antagConflict`) symmetrically regardless of ConflictType. Schema + `OutlineBuilder.BuildProblemProperties` extended.
- `b09e0ff` — `.gitignore` excludes `*.docx`, `*.pdf`, `*.txt`, `*.stbx`, `*.raw.json`, `*.costs.json`, `*.rating.json` under `samples/Outliner/Outliner/` and `samples/Outliner/OutlinerTests/`.
- End of session: branch at `b09e0ff`, 18 `(#13)` commits ahead of main, 44/44 tests passing.

### 2026-05-11 — NuGet flip attempt (abandoned)
- Attempted to flip `Directory.Build.props` `UseStoryCADLibNuGet` to `true` and bump `StoryCADLibVersion` to 4.1.0; removed sibling-clone `ProjectReference` fallback from 11 consumer csprojs.
- Build failed `NU1202`: published `StoryCADLib 4.1.0` shipped only `net10.0-desktop1.0`; Outliner targets `net10.0-windows10.0.22621`. 4.0.2 had both TFMs; 4.1.0 was packed missing the Windows TFM.
- Local workarounds (changing Outliner to `net10.0-desktop`) failed both ways: `Microsoft.NET.Sdk` rejected the TPI (`NETSDK1139`); `Uno.Sdk` build hit `NETSDK1136` because `UseWinUI` + `WindowsAppSDKSelfContained` pin to a `-windows` TFM. Port from WindowsAppSDK XAML to Uno.UI XAML already ruled out.
- Outliner.csproj edits reverted; 11 consumer csproj edits + `Directory.Build.props` change left uncommitted pending the upstream republish.

### 2026-05-15 morning — PR #17 and PR #18 merged into main
- **PR #17 (NuGet flip)** — StoryCADLib 4.1.1 published with both `net10.0-desktop` and `net10.0-windows10.0.x` assets, fixing the missing-TFM issue from 2026-05-11. Merged at `3978e62`: `UseStoryCADLibNuGet=true`, `StoryCADLibVersion=4.1.1`, sibling-clone fallbacks removed across all 11 consumers. Verified 11/11 build (Outliner needs `-p:Platform=x64`), StoryCADMcp.Tests 88/88, OutlinerTests 29/29.
- **PR #18 (description fields)** — fixed empty description fields on Problems, Characters, Settings. Root cause: `OutlineBuilder` wrote property keys absent from the element models; `UpdateElementProperty` throws via reflection, `UpdateElementProperties` short-circuits, `AddElement` (`StoryCADAPI.cs:615`) ignores the result — every later key silently dropped. The four labelled UI fields ("Story Question", "Character Sketch", "Setting Summary", "Scene Sketch") all bind to base `StoryElement.Description`. Merged at `1d5a1d1` (commit `93c16d4`). Filed upstream `storybuilder-org/StoryCAD#1409` against the swallowed return value at `StoryCADAPI.cs:615`.

### 2026-05-15 afternoon — Rebase recovery
- Branch-survey miss: PR #18 turned out to be duplicate work — `issue-13-outliner-hardening` already contained the equivalent fix in `AddIfPresent`-style code. PR #17 was genuinely new; this branch still conflicted with it on Outliner.csproj.
- Created backup `issue-13-outliner-hardening-backup` at `797c5b9`. Rebased 19 commits onto main. Two conflicts resolved in favor of this branch:
  - `OutlineBuilder.cs` — this branch's imperative builder kept.
  - `OnePassSystemPrompt.md` — this branch's full rewrite kept.
- `OutlinerTests/OutlineBuilderTests.cs` had been deleted on this branch in favor of VM-targeted tests; the 9 description-field tests PR #18 added are gone with the file.
- Semantic gap caught: after the rebase, the builder still routed `CharacterSketch` / `Summary` / `StoryQuestion` to `Notes`, not `Description`. Commit `7ccb24e` reroutes them to `Description`. `Relationships` stays in `Notes` (no structured destination).
- Force-pushed to `origin/issue-13-outliner-hardening` at `7ccb24e`.

### 2026-05-16 — PR #19 merged into main
- PR #19 opened from `issue-13-outliner-hardening → main` marked `Part of #13` (issue stays open). Merged at `a94474c` via merge commit; remote branch deleted via `--delete-branch`.
- Local cleanup: fast-forwarded local `main`; deleted `issue-13-outliner-hardening` (safe-delete) and `issue-13-outliner-hardening-backup` (force-delete; pre-rebase SHAs not in main, reflog ~90 days).
- Status comment posted on issue #13; acceptance-criteria checkboxes untouched pending runtime verification.

### 2026-05-16 — Deterministic integration test restored; live test moved to ManualTests
- Investigated the open carry-forward "deterministic integration test for CI." Found that the equivalent test had existed (`StubbedPipelineTests.cs` + `FakeChatCompletionService.cs` + `Fixtures/Mirror, Mirror.raw.json`) and was deleted on this branch in commit `6913142` ("replace stubbed pipeline tests with VM tests"). Deletion rationale conflated VM-test coverage with pipeline-test coverage; removing the stubbed tests removed the only CI-runnable pipeline coverage.
- The surviving `EndToEndOutlineTests.cs` tagged `[TestCategory("LiveLLM")]` is a manually-invoked smoke check, not a CI-runnable test. An excluded-by-default test isn't a unit test.
- Restored the three deleted files from `6913142^`:
  - `samples/Outliner/OutlinerTests/StubbedPipelineTests.cs`
  - `samples/Outliner/OutlinerTests/FakeChatCompletionService.cs`
  - `samples/Outliner/OutlinerTests/Fixtures/Mirror, Mirror.raw.json`
- Modified `StubbedPipeline_MirrorMirror_ProducesExpectedElements` to call `RunAsync(prosePath, outputPath)` against a synthetic temp `.txt` written into `TestOutputs/`, so `ProseDocumentReader` is exercised on the way in. Other tests stay on `RunFromTextAsync` (LLM is stubbed; prose content irrelevant).
- `App.xaml.cs` — added `FixturesDir` (next to the `.csproj`) plus a `Directory.CreateDirectory(FixturesDir)` call.
- `.gitignore` — added a scoped exception `!samples/Outliner/OutlinerTests/Fixtures/**` so the recorded `.raw.json` fixture isn't caught by the broad `*.raw.json` ignore added in `b09e0ff`.
- Deleted `samples/Outliner/OutlinerTests/EndToEndOutlineTests.cs`. The `LiveLLM` test category went with the file.
- Replaced it with a manual-test checklist `samples/Outliner/OutlinerTests/ManualTests/Outliner_End_To_End_Test.md` modeled on `StoryCAD/StoryCADTests/ManualTests/Smoke_Test.md`. Covers launch, prose read, outline generation, per-run artifacts, `.stbx` round-trip in StoryCAD, content spot-check, missing-API-key error path.
- Build clean. Test run: **56/56 passing** (previous 52 + 4 restored stubbed-pipeline tests).
- Merged via PR #20 at `00947bb` (merge commit); remote and local branches deleted.

### 2026-05-16 — Prompt v4: ScenePurpose, StoryRole, Person-vs-Self enforcement, Sketch vs Appearance
- Five gaps surfaced from the manual end-to-end test against "Mirror, Mirror":
  1. `Scene.ScenePurpose` never populated — not in prompt, not in schema, not in builder.
  2. Problem with `conflictType: "Person vs. Self"` had a separate character as Antagonist instead of Antagonist == Protagonist.
  3. Scene Conflict tab had the same Person-vs-Self mis-assignment (separate Antagonist character).
  4. Character Sketch and Appearance contained identical physical-description text.
  5. `Character.StoryRole` (fixed-list narrative role) was empty; the LLM was emitting narrative-role values into `role` (free-form world role).
- Fix scope:
  - `OnePassResponse.cs` — added `CharacterElement.StoryRole` (string) and `SceneElement.ScenePurpose` (List&lt;string&gt;).
  - `OutlineBuilder.cs` — `BuildCharacterProperties` maps `StoryRole`; `AddScenes` loops `ScenePurpose` and writes via `AddCollectionEntry` (same pattern as `CastMembers`).
  - `OnePassSystemPrompt.md` — JSON shape clarifies `role` (free-form) vs `storyRole` (fixed list of 6 canonical values) and `characterSketch` (role/age summary) vs `appearance` (physical only). New rule sections: "Scene purpose — required, multi-select" (9 canonical values from `StoryCADLib/Assets/Install/Lists.json`), "Character story role — fixed list", "Character sketch vs. appearance — keep them distinct". "Person vs. Self conflicts" rule strengthened to absolute and extended to every Scene serving a PvS problem. Worked example updated.
  - `OutlineRating.cs` — `OutlinePrompt.Version` bumped to "v4".
- Test infra:
  - Updated `Fixtures/Mirror, Mirror.raw.json` with v4 fields (Jaime: role "Student" + storyRole "Protagonist" + sketch revised; Possibility: role "AI Mirror" + storyRole "Supporting Role"; scene: scenePurpose `["Introduce Situation", "Develop Characters"]`).
  - Added three deterministic tests in `StubbedPipelineTests`: `CharacterStoryRoleAndRolePopulated`, `ScenePurposePopulated`, `CharacterSketchDistinctFromAppearance`.
- Build clean. Test run: **59/59 passing** (was 56; +3 new v4 tests).
- Live-LLM verification deferred to the next manual run. Schema/builder coverage is deterministic; whether the LLM honors the strengthened prompt rules under the v4 instructions needs an actual run against a real model.
- Merged via PR #21 at `a771b0e` (merge commit); remote and local branches deleted. Manual re-run against "Mirror, Mirror" verified the five fixes before merge.

### 2026-05-21 — GUID regeneration fix (found during #14 work)

**Done**
- `0b0d370` — `ProseAnalyzer.EnsureValidGuids` now unconditionally regenerates every element GUID via `Guid.NewGuid()` and remaps every cross-reference, instead of only regenerating when `Guid.TryParse` fails. Committed on branch `issue-14-critter-rebuild` (not a #13-specific branch).

**Worked / didn't**
- Cause: the LLM emits syntactically valid placeholder GUIDs (`11111111-1111-1111-1111-111111111111`, `22222222-…`) that pass `Guid.TryParse`; `OutlineBuilder` passed them to `AddElement` as the element UUID, so every Outliner-generated `.stbx` shipped with canonical UUIDs. Confirmed in a live `Mirror, Mirror_outline.raw.json` (raw LLM output before post-processing).
- OutlinerTests 59/59 still pass after the change.
- Verified at code/test level only; no live Outliner run done this session to confirm a fresh `.stbx` carries real GUIDs.

**Remains**
- Pre-existing `.stbx` files keep their canonical UUIDs; only new Outliner runs produce real GUIDs.
- Fix is committed on the `issue-14-critter-rebuild` branch, not yet pushed/merged.

**New tasks / issues**
- Decide whether this fix should land via its own branch/PR rather than riding the #14 branch.

### 2026-05-22 — Public-domain test fixture (found during #14 work)

**Done**
- `d2e1519` — replaced the stubbed-pipeline fixture `Fixtures/Mirror, Mirror.raw.json` with `Fixtures/The Yellow Wallpaper.raw.json` (Charlotte Perkins Gilman, 1892, public domain). Same shape the tests exercise: 2 characters, 1 setting, 1 scene, 1 Person-vs-Self problem (antagonist GUID == protagonist GUID), scene purposes "Introduce Situation"/"Develop Characters", overview StoryType/StoryGenre, distinct sketch vs appearance. List values validated against `StoryCADLib/Assets/Install/Lists.json` (StoryType "Short Story", Genre "Literary"). Rewrote `StubbedPipelineTests.cs`: repointed to the new fixture, updated assertions (Narrator/John, Patient/Physician), and changed the source-doc-name argument so no proprietary string remains in test code. Committed on branch `issue-14-critter-rebuild`.

**Worked / didn't**
- OutlinerTests 59/59 pass with the new fixture.
- Driver: the repo is slated to go public at the 4.2 release (#15); proprietary stories can't ship in tracked files.

**Remains**
- None for this item.

**New tasks / issues**
- None.

## Carry-forward for #13 completion

Open acceptance criteria:
- [ ] Sensible error handling in `ProseAnalyzer` for missing/invalid `OPENAI_API_KEY`, 429 rate-limit, network, 5xx, empty/huge file, malformed schema response. Currently wraps everything as `InvalidOperationException("Failed to analyze prose: ...")`.
- [ ] `.stbx` round-trip verification in StoryCAD without warnings (covered by the manual test plan; needs an actual run).
- [x] Deterministic integration test — `StubbedPipelineTests` restored 2026-05-16; 4 tests pass against recorded `Mirror, Mirror.raw.json` fixture.
- [ ] CI workflow for tests on push to `main` / `dev`. Only `.github/workflows/deploy-docs.yml` exists today; per Terry, adding a CI pipeline for samples is unlikely but worth a conversation with Jake. Manual test plan now stands in for the live-LLM half.
- [ ] `samples/Outliner/Outliner/README.md` rewrite. Current README pre-dates this branch and references stale prompt filenames (`StoryCAD-SystemPrompt.md`, `OutliningHeuristics.md` vs actual `OnePassSystemPrompt.md`); needs API key requirement, runtime expectations on small/medium/large inputs, known limitations, single vs batch modes, cost-capture artifacts.

Related sibling work:
- Issue #15 item 2 — Help button on Outliner nav: not started.
