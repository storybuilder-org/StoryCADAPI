# Issue #13 — Outliner sample hardening — status log

Sub-issue of storybuilder-org/StoryCAD#1246.

## Branch

`issue-13-outliner-hardening` — branched from `issue-1246-repo-reorg` before that branch was merged to `main` and deleted. Currently has uncommitted WIP only.

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

#### Prompt gaps still open

- StoryType, StoryGenre, StoryProblem on Overview not requested by prompt.
- Person-vs-Self convention not enforced — LLM made Possibility the Antagonist on the Problem and the Scene; per StoryCAD convention, both should be Jaime.
- "goals" and "motives" emitted as combined strings on Problem rather than the split ProtGoal / ProtMotive / ProtConflict / AntagGoal / AntagMotive / AntagConflict the model expects.
- GUIDs not specified as hex-only (defensive code regenerates invalid ones, but tightening the prompt avoids the regenerate path).

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
