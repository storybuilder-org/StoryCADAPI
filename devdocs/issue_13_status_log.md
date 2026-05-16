# Issue #13 — Outliner sample hardening — status log

Sub-issue of storybuilder-org/StoryCAD#1246.

## Branch

`issue-13-outliner-hardening` — branched from `issue-1246-repo-reorg` before that branch was merged to `main` and deleted. Pushed to `origin/issue-13-outliner-hardening`.

## IP exposure on the remote branch — assessed and accepted (2026-05-07)

Multiple commits on this branch contain prose `.docx` files (`Mirror, Mirror.docx`, `Mister Death.docx`, `The Long Ride Home.docx`) that were committed and pushed to `origin/issue-13-outliner-hardening` before the gitignore rules were in place.

User assessment (2026-05-07): own stories aren't a concern; *Mister Death* is not user IP and is already publicly available. **Decision: leave the existing committed copies as-is.** No history rewrite, no force push.

Affected commits (each contains one or more of the .docx files):
- `dc10d33` (initial Outliner test infra)
- `03ee02a` (folder rename / move into Outliner project)
- `0149e47` (prompt v3 — also moved files between OutlinerInput and OutlinerStories)

Forward protection committed in `b09e0ff` (2026-05-07):
- `.gitignore` excludes `*.docx`, `*.pdf`, `*.txt`, `*.stbx`, `*.raw.json`, `*.costs.json`, `*.rating.json` under both `samples/Outliner/Outliner/` and `samples/Outliner/OutlinerTests/`.
- The four `.docx` files that were tracked are now untracked (still on disk).
- The status log itself moved out of the repo to `/mnt/c/temp/issue_13_status_log.md` so future status-log changes don't get committed.

## What's done

### Repo / branch hygiene (2026-05-05)
- `issue-1246-repo-reorg` merged to `main` (no PR; merge commit `3c4d35d`) as the baseline for #1246.
- `issue-1246-repo-reorg` deleted from local and `origin`.
- Outliner WIP carried forward onto `issue-13-outliner-hardening`.

### Build resolved (2026-05-05 → 2026-05-06)
- Diagnosed VS NU1105 as a cross-SDK ProjectReference issue (Outliner uses `Microsoft.NET.Sdk`, StoryCADLib uses `Uno.Sdk`) compounded by StoryCADLib not being in `Outliner.sln`.
- Fix applied (2026-05-06): added StoryCADLib to `samples/Outliner/Outliner.sln` via `dotnet sln`. Solution now builds in VS without a CLI restore dance.

### Test infrastructure fix (2026-05-06)
Stale files were lingering in `bin\...\TestInputs\` after they were removed from source, because `<None Update="TestInputs\*"><CopyToOutputDirectory>Always</CopyToOutputDirectory></None>` doesn't delete files removed from source. Result: tests ran against three stories when only one was in `TestInputs/`.

Changes (uncommitted):
- `samples/Outliner/OutlinerTests/App.xaml.cs` — `InputDir` and `OutputDir` now resolve to the source `TestInputs/` and `TestOutputs/` next to `OutlinerTests.csproj`, via a `FindProjectDir()` walk-up from `AppContext.BaseDirectory`.
- `samples/Outliner/OutlinerTests/OutlinerTests.csproj` — removed the four `<None Update="TestInputs\*">` copy entries and the empty `<Folder Include="TestOutputs\" />`.
- `samples/Outliner/OutlinerTests/TestSetup.cs` — clears all `*.stbx` from `TestOutputs/` at assembly init (replaces the per-test delete).
- `samples/Outliner/OutlinerTests/EndToEndOutlineTests.cs` — removed the per-test `File.Delete(outputPath)` since `TestSetup` now handles it.

User folder convention now in effect:
- Source `TestInputs/` — prose to process.
- Source `TestStories/` — prose held aside, not processed.
- Source `TestOutputs/` — generated `.stbx` outlines (cleared at start of every run).

### Test run (2026-05-06)
- Built via `cmd.exe /c C:\temp\build_outliner.cmd` (split MSBuild Restore + Build, x64 Debug).
- Ran via `cmd.exe /c C:\temp\run_outliner_tests.cmd` (vstest.console.exe against the test DLL).
- **First run: 30 tests, 29 passed, 1 failed** — `Pipeline: Mirror, Mirror.docx` failed with `HTTP 429 (insufficient_quota)`. OpenAI account quota issue, not a code defect.
- **After OpenAI account top-up, second run: 30/30 passed.** End-to-end pipeline took 9s for Mirror, Mirror.docx; produced `samples/Outliner/OutlinerTests/TestOutputs/Mirror, Mirror.stbx` (4728 bytes). Open in StoryCAD to judge outline quality.

## What's still open for #13 acceptance criteria

### Hardening / production-ready
- [x] Builds cleanly on a fresh checkout against current StoryCADLib.
- [x] End-to-end happy path verified — Mirror, Mirror.docx produces a structurally complete `.stbx` with rich field content. Final state after the 2026-05-06 fixes:
  - **5 content elements** (was 3): Jaime, Possibility, Setting, Problem, Scene.
  - **StoryOverview**: Title, Author, Premise populated. Still blank (prompt gap): StoryType, StoryGenre, StoryProblem, Concept, Viewpoint.
  - **Character (Jaime)**: Role, Age, Sex, Eyes, Appearance, PsychNotes populated. CharacterSketch + Relationships folded into Notes.
  - **Setting**: Locale, Lighting, Sights, Sounds populated. Summary folded into Notes.
  - **Scene**: ViewpointCharacter, Setting, CastMembers (both characters), Protagonist, ProtagGoal, Antagonist, AntagGoal, Outcome, ElementDescription all populated.
  - **Problem**: ProblemType, ConflictType ("Person vs. Self"), ProblemCategory, ProblemSource, Protagonist, Antagonist, Outcome populated. StoryQuestion folded into Notes.

#### Root causes diagnosed and fixed (2026-05-06)

1. **JSON deserialization** — `OnePassResponse.cs` had snake_case `[JsonPropertyName]` attributes (`character_sketch`, `prot_goal`, `viewpoint_character`, etc.) on every inner field, but the LLM emits camelCase. Case-insensitive matching doesn't bridge underscores. Fix: removed inner attributes; case-insensitive matching now resolves `protGoal` → `ProtGoal` correctly. Kept the one explicit `[JsonPropertyName("story_overview")]` at the top level.
2. **Invalid GUIDs from LLM** — LLMs frequently emit GUID-shaped strings with non-hex characters (e.g., `a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6` containing g, h, i, j, k, l, m, n, o, p). `Guid.Parse` rejects these silently and the element is dropped. Fix: `ProseAnalyzer.ValidateGuids` now validates with `Guid.TryParse`, regenerates fresh GUIDs for invalid ones, and remaps every cross-reference (Scene.Protagonist/Antagonist/ViewpointCharacter/Setting/Cast[], Problem.Protagonist/Antagonist) so links survive.
3. **OutlineBuilder field-name mapping** — Several property dictionaries used names that don't exist on the StoryCAD models: Character had `CharacterSketch` and `Relationships` (string); Scene used `ProtGoal` (should be `ProtagGoal`) and `Cast` (should be `CastMembers`, plus it's a List that needs `AddCollectionEntry` rather than `UpdateElementProperty`); Problem had `StoryQuestion` and `Significance`; Overview had `Title`. Combined with `UpdateElementProperties` bailing on the **first** failed property, this dropped most of the dictionary. Fix: `Build*Properties` now use only valid StoryCAD field names; non-mapping LLM data folds into Notes; CastMembers populated via `AddCollectionEntry`.

#### Prompt gaps still open (post-v1)

- StoryType, StoryGenre, StoryProblem on Overview not requested by prompt.
- Person-vs-Self convention not enforced — LLM made Possibility the Antagonist on the Problem and the Scene; per StoryCAD convention, both should be Jaime.
- "goals" and "motives" emitted as combined strings on Problem rather than the split ProtGoal / ProtMotive / ProtConflict / AntagGoal / AntagMotive / AntagConflict the model expects.
- GUIDs not specified as hex-only (defensive code regenerates invalid ones, but tightening the prompt avoids the regenerate path).

### NavigationView shell, Batch + Settings pages, MVVM test rework (2026-05-06 → 2026-05-07)

Commits added since the prompt-v2 commit:

- `9aa287a` refactor — extracted `OutlineRunner` (read → analyze → build → write four artifacts) so the single-mode UI, batch-mode UI, and tests share one pipeline.
- `b466a51` test — split tests into `[TestCategory("LiveLLM")]` for live tests + a stubbed pipeline test class. (Later removed in `cb68055`.)
- `66909a7` feat — `OutlinerPreferences` + `PreferencesService` persisted to `%LocalAppData%/Outliner/preferences.json`. Registered in DI.
- `b406521` refactor — replaced `MainWindow` content with a `NavigationView` shell + `Frame`. `ItemInvoked` handler routes by Tag.
- `19a64a0` feat — `BatchPage` + `BatchPageViewModel`. Folder picker, per-file progress list, summary line, runs `OutlineRunner` per file, continues on errors. Outputs default to `<input>/Outlines/`.
- `215c1d9` feat — `SettingsPage` + `SettingsPageViewModel` (footer entry). Startup mode radio.
- `cb68055` test — replaced the stubbed pipeline tests (redundant with the live tests at the same code paths) with three VM-targeted test classes (`ContentPageViewModelTests`, `BatchPageViewModelTests`, `SettingsPageViewModelTests`). Aligned with the shop's MVVM-test convention. `DispatcherQueue` removed from VMs (StoryCAD's VMs don't use it; async/await captures the SynchronizationContext naturally). `InternalsVisibleTo` is **not** used; tests touch only public surface.
- `03ee02a` refactor — moved test fixture folders into the Outliner project with neutral names: `OutlinerInput/`, `OutlinerOutput/`, `OutlinerStories/`. Tests reference them in the sibling project.
- `5d81d57` feat — added a "Pick Output Folder…" button to BatchPage so the user can override the default `<input>/Outlines/`. Added two folder pickers to SettingsPage (`DefaultInputFolder`, `DefaultOutputFolder`) shared by both modes. Consolidated four per-mode folder fields in `OutlinerPreferences` down to two shared defaults + the batch last-used pair.
- `985ddae` fix — `Story Content` textbox was bound to `ContentText` (status accumulator) instead of `StoryText` (the prose). Rebound to `StoryText`; status messages moved to a separate gray TextBlock. Also: tests now write to `OutlinerTests/TestOutputs/` (test-only sandbox) instead of sharing `Outliner/OutlinerOutput/` with the user-facing app. `MainWindow` switched from `SelectionChanged` to `ItemInvoked` for nav routing.
- `0149e47` feat — prompt v3. Adds `subject`, `theme`, `method` to every problem, plus all six goal/motive/conflict slots (`protGoal`/`protMotive`/`protConflict` + `antagGoal`/`antagMotive`/`antagConflict`) symmetrically on every problem regardless of conflict type. Schema (`OnePassResponse.ProblemElement`) + `OutlineBuilder.BuildProblemProperties` extended. `OutlinePrompt.Version` bumped to `v3`.

Test count: **44 / 44 passing** (29 service-unit + 4 ContentPageVM + 4 BatchPageVM + 4 SettingsPageVM + 1 live LLM + 2 DI smoke = 44; the 1 live LLM test is `[TestCategory("LiveLLM")]`).

### Prompt v3, file-handling fixes, and IP cleanup (2026-05-07 → 2026-05-08)

Branch is at `b09e0ff`, pushed to origin. Eighteen `(#13)` commits ahead of `main`. 44 / 44 tests passing.

Commits since the prior log entry:
- `5d81d57` feat — BatchPage gained a "Pick Output Folder…" button that overrides the default `<input>/Outlines/`. Settings page added two folder pickers (`DefaultInputFolder`, `DefaultOutputFolder`) shared by both modes. `OutlinerPreferences` schema simplified from four per-mode folder fields to two shared defaults + `LastBatchInputFolder` / `LastBatchOutputFolder`.
- `985ddae` fix — `Story Content` textbox in single mode was bound to `ContentText` (status accumulator) instead of `StoryText` (the actual prose). After "Read Story" the user saw "Loaded: filename.docx" in place of the prose. Rebound to `StoryText`; status messages moved to a separate gray TextBlock. Tests now write to `OutlinerTests/TestOutputs/` (test-only sandbox) so they don't pollute the user-facing `Outliner/OutlinerOutput/`. MainWindow nav switched from `SelectionChanged` to `ItemInvoked`.
- `0149e47` feat — prompt v3. Adds `subject`, `theme`, `method` to every problem, and forces all six goal/motive/conflict slots (`protGoal`/`protMotive`/`protConflict` + `antagGoal`/`antagMotive`/`antagConflict`) symmetrically on every problem regardless of conflict type. Schema (`OnePassResponse.ProblemElement`) and `OutlineBuilder.BuildProblemProperties` extended. `OutlinePrompt.Version` bumped to `v3`.
- `b09e0ff` chore — the IP-protection commit described in the section above.

### Outstanding for #13 (carry forward)

**From the original acceptance criteria, what remains:**
- **Sensible error-handling distinctions in `ProseAnalyzer`** — currently wraps everything as `InvalidOperationException("Failed to analyze prose: ...")`. Doesn't distinguish missing API key, 429 quota, 5xx, network, malformed-JSON. Issue #13 explicitly lists those as required.
- **`samples/Outliner/README.md` rewrite** — current README predates this branch's work. Doesn't mention single vs batch modes, cost capture, prompt v3 structure, the four output artifacts, or the actual run cost expectations.
- **CI for tests on every push** — there's only `deploy-docs.yml` in StoryCADAPI today, and it builds docs not tests. Discussion deferred pending Jake's direction (post-dissertation). Not a #13 hard blocker; better solved as a separate sub-issue under #1246.

**Live-quality observations on Mirror, Mirror v3 (2026-05-07):**
- v3 successfully fills the six goal/motive/conflict slots and Person-vs-Self enforces Antagonist == Protagonist. Verified via MCP read of the produced `.stbx`.
- The auto-rating completeness metric is partly perverse (more requested fields → lower ratio even when quality goes up). Long-term replacement is a domain-truth evaluator, deferred to the eval loop work.

### Prompt v2 (2026-05-06)

`OnePassSystemPrompt.md` rewritten. `OutlinePrompt.Version` bumped to "v2" so future ratings can be partitioned by prompt revision. The schema added `StoryType`, `StoryGenre`, `StoryProblem`, `Concept` to `StoryOverviewElement`; OutlineBuilder maps them through to OverviewModel.

v2 closes every user-flagged gap on the Mirror, Mirror reference fixture:
- **StoryType "Short-Short"** ✓
- **StoryGenre "Science Fiction"** ✓
- **Concept** populated ✓
- **StoryProblem** points at the Story-problem GUID ✓
- **Premise** generated using the explicit template ✓
- **Person vs. Self** antagonist rule now enforced — both Protagonist and Antagonist on the Problem are Jaime ✓
- **Hex-only GUID rule** explicit ✓
- **Always-emit-a-Setting** rule explicit ✓
- **Worked example** added to anchor depth ✓

Auto-rating: completeness 0.655, thumbs_up. The drop from 0.925 (v1) to 0.655 (v2) is a denominator effect, not a quality regression — adding requested fields raises the ceiling. **The completeness heuristic is partly perverse**: asking for more reduces ratios even when domain quality improves. A better long-term signal is a domain-truth evaluator (deferred until the eval loop is built).

#### Reference target: what Mirror, Mirror's outline *should* contain (per Terry, 2026-05-06)

A correct outline of the prose fixture should produce:

- **StoryOverview** — Title "MIRROR, MIRROR", Author "T. W. Cox", **StoryType "Short-Short"**, **StoryGenre "Science Fiction"**, **Premise** generated from StoryCAD's premise template, **StoryProblem** set to the single Problem element ("Jaime's struggle with identity").
- **Characters (two, currently only one)** — Jaime; **Possibility** (the AI mirror) is missing entirely.
- **Problem "Jaime's struggle with identity"** — **ConflictType: Person vs. Self**, **Protagonist: Jaime**, **Antagonist: Jaime**. (This Problem also lifts to the Overview's StoryProblem.)
- **Scene "Jaime confronts his reflection"** — **ViewpointCharacter: Jaime**, **Cast: [Jaime, Possibility]** (currently one slot resolves and one shows as `(none)`), **Setting: [the men's-room smart-wall mirror]** (currently absent).
- **Setting** — the men's-room smart-wall mirror (currently no Setting element generated at all).

These are the concrete acceptance gaps for the prompt + `OutlineBuilder` mapping work under the "production-ready" criteria.
- [ ] Sensible error handling on real failure modes (missing API key, 429/5xx, empty file, malformed LLM response). `ProseAnalyzer` currently catches everything as a generic `InvalidOperationException("Failed to analyze prose: ...")`, hiding 429-vs-network-vs-bad-JSON.
- [ ] Output `.stbx` opens correctly in StoryCAD without warnings (needs a real run).
- [ ] User-visible progress / status (slow LLM round-trip).

### Tests beyond DI smoke
- [x] Unit-level tests for response-parsing logic — present in `ProseAnalyzerTests.cs` and `OutlineBuilderTests.cs`.
- [ ] Integration test against a small fixture + recorded LLM response or local stub. The `EndToEndOutlineTests` exercises the pipeline but requires a live LLM key — needs a stubbed-LLM mode for CI.
- [ ] CI on every push to `main` / `dev`.

### Documentation
- [ ] `samples/Outliner/README.md` describing inputs, API key requirement, expected runtime, known limitations.
- [ ] Pro-perk monetization update (if that decision lands before this issue closes).

## Files added on this branch (still untracked)

- `samples/Outliner/Outliner/Services/ProseDocumentReader.cs`
- `samples/Outliner/OutlinerTests/EndToEndOutlineTests.cs`
- `samples/Outliner/OutlinerTests/TestStories/Mister Death.docx`
- `samples/Outliner/OutlinerTests/TestStories/The Long Ride Home.docx`

## How to build and run tests from CLI

Build (from WSL):
```
cmd.exe /c "C:\temp\build_outliner.cmd"
```

Run tests (from WSL):
```
cmd.exe /c "C:\temp\run_outliner_tests.cmd"
```

Both batch files live in `C:\temp\` and follow the StoryCADAPI build recipe (split MSBuild Restore + Build, vstest.console against the DLL).

## Known oddity worth tracking

VS in-IDE NuGet restore fails with NU1105 the first time after a fresh `obj/` wipe, because of the cross-SDK ProjectReference. CLI `dotnet restore Outliner.sln` produces the assets file VS needs, after which VS works for that session. Adding StoryCADLib to `Outliner.sln` (done) reduces but does not fully eliminate this — a CLI restore may still be needed once after deep cleans on some machines.
