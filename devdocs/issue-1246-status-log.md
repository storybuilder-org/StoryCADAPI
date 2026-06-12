# Issue #1246 Status Log — Update and release new samples and API guidance

Repo: storybuilder-org/StoryCAD
Issue: https://github.com/storybuilder-org/StoryCAD/issues/1246
Branch: `issue-1246-repo-reorg` (in StoryCADAPI)
Phase: Test tasks / sign-off (in progress). StoryCADAPI #7 (MCP collection-entry tools) — code complete on its own branch.

## Where we are (2026-05-01)

**Working build/test loop is established.** From WSL:
1. Build via cmd.exe wrapper (Windows-native working directory).
2. Split MSBuild into separate Restore and Build calls (combining them as `-t:Restore,Build` breaks Uno.Sdk + IsTestProject — was the cause of all morning's `TestContext` confusion).
3. Run tests with `vstest.console.exe` against the built DLL.

Recipe and full command lines saved in memory: `reference_storycadapi_build_test.md`.

## Build status (full repo, default toggle off)

9 of 12 projects build cleanly:
- StoryCADChat, StoryCADCli, StoryCADMcp, HeadlessTest
- 5 simple samples (AutomatedCritique, ConsistencyValidation, StoryDiagnosticAgent, StoryGraphBasics, StoryMetrics)

3 still fail (all pre-existing, not caused by this session's work):
- **StoryCADMcp.Tests** — fails with `-t:Restore,Build` combined; passes with split. 72 tests pass when run via the working recipe.
- **samples/Outliner/Outliner** — Uno source generator emits code referencing `Uno.UI.Xaml`; project is plain WinUI (Microsoft.NET.Sdk), not Uno. Pre-existing.
- **samples/Outliner/OutlinerTests** — depends on Outliner.csproj plus needs explicit `-p:Platform=x64`. Pre-existing.

## Changes made this session

### Toggle for StoryCADLib reference
- Created `Directory.Build.props` at the **repo root** (was originally at `samples/`, then promoted).
- Defines `UseStoryCADLibNuGet` (default `false`) and `StoryCADLibVersion` (`4.0.2`).
- Applied the conditional ProjectReference / PackageReference pair to 11 csprojs:
  - 5 simple samples
  - samples/Outliner/Outliner
  - StoryCADChat, StoryCADCli, StoryCADMcp, StoryCADMcp.Tests, HeadlessTest
- Default behavior is unchanged from before (still uses local StoryCAD repo).
- Toggle is dormant until a StoryCADLib NuGet ≥ 4.0.3 is published with the indexed-list / move-element fixes — then flipping `UseStoryCADLibNuGet=true` makes the repo buildable without needing the StoryCAD repo cloned.

### Uno.Sdk version unification
- Bumped `Uno.Sdk` from `6.4.53` to `6.5.31` in 6 `global.json` files (5 simple samples + HeadlessTest).
- Brings them in line with the rest of StoryCADAPI and with StoryCAD's main/dev branches.

### Removed orphan starter project
- Deleted `Blank Project/StoryCAD-API-Starter/` (out of date, not referenced anywhere in docs/website, not used).
- Backup at `/mnt/c/temp/Blank Project/`.
- 3 tracked files staged for deletion.

### Outliner WIP changes
- The 11 unstaged Outliner files from before this session are still uncommitted. Out of scope for this work.

## Decisions / open questions

### NuGet vs ProjectReference for samples
- Long-term: at StoryCAD 4.2 release, all samples + StoryCADMcp consume StoryCADLib from NuGet.
- Until then, default stays on ProjectReference. Toggle is in place for the eventual flip.
- Outliner's NuGet flip (in unstaged changes) is also gated by the same toggle now.

### TFM
- All 9 projects that previously targeted `net10.0-desktop` were briefly flipped to `net10.0-windows10.0.22621` in this session, then **reverted**. They are back at their original TFMs.
- Mac support is live (Apple App Store + Microsoft Store). Cross-platform `net10.0-desktop` remains intentional. See memory `project_storycad_platforms.md`.

## StoryCADAPI #7 — code complete

Branched off `main` as `issue-7-mcp-collection-entry-tools` (no overlap with #1246's files; can land independently).

**Done:**
- Three new tools added to `StoryCADMcp/Tools/WriteTools.cs`: `add_collection_entry`, `update_collection_entry`, `remove_collection_entry`. All take entry as a JSON string and parse to `JsonElement` before calling the API.
- New test file `StoryCADMcp.Tests/CollectionEntryToolsTests.cs` with 16 tests (happy paths, no-outline, invalid GUID, invalid JSON, bad property name, out-of-range index).
- Test suite: 88 tests, 88 passing.
- Single commit: `19a91cb`.
- Issue #7 body updated; comment posted with progress; Evaluate phase plan/approval ticked.

**Outstanding:**
- Update `StoryCADMcp/README.md` to list the new tools.
- Real-world MCP testing with a live MCP client (Claude Desktop) against a real outline.
- Open PR.

**Lessons learned from this session** captured at `/mnt/c/temp/storycadapi-lessons-learned.md`.

## Context from handoff (`/mnt/c/temp/issue_7_handoff_from_1379.md`)

- StoryCAD branch: `issue-1379-complete-storycadapi-interface`, commit `09897730`, PR #1394 awaiting review.
- StoryCAD tests: 991 / 977 passed / 14 skipped / 0 failed (Windows + macOS verified).
- `UpdateElementProperty` on a `List<T>` property now returns redirect message.
- Merge order: StoryCAD #1394 → `dev`, then StoryCADAPI #7 → `dev`. Both ride Release 4.1.

## Related memory
- `project_storycadlib_release.md` — StoryCADLib 4.0 beta, blocked by Apple cert + move-element; NuGet update deferred
- `project_deferred_work.md` — Pending: MCP docs, AddElementWithProperties docs, NuGet publish
- `project_storycad_platforms.md` — Mac live; TFM strategy
- `feedback_tfm_choice.md` — Never silently strip net10.0-desktop
- `feedback_plain_english.md` — Talk to user in plain English
- `reference_msb4216_fix.md` — MSB4216 EmbeddedResourceInjectorTask fix
- `reference_storycadapi_build_test.md` — How to build and test StoryCADAPI from CLI (split Restore/Build, vstest.console.exe)
