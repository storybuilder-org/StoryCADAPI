# Issue #1246 — NuGet 4.0.2 & Outliner Status Log

## NuGet Publish (2026-04-01, completed)

### What was done
1. Bumped Version/AssemblyVersion/FileVersion in StoryCADLib.csproj from 4.0.0 to 4.0.2
2. Built .nupkg with this exact command (from PowerShell on Windows, in D:\dev\src\StoryCAD):
   ```
   dotnet pack StoryCADLib/StoryCADLib.csproj -c Release -o C:\temp\nuget -p:IsPackable=true
   ```
3. Output: `C:\temp\nuget\StoryCADLib.4.0.2.nupkg`
4. Pushed to NuGet.org — published 2026-04-01 at 12:23
5. Reverted version bump in StoryCADLib.csproj (was on wrong branch — issue-1332-discord-reorganization)

### Key findings
- **Uno SDK sets IsPackable=false by default** — must pass `-p:IsPackable=true` to produce a .nupkg
- **NU5104 warning**: `Microsoft.SemanticKernel.Planners.OpenAI` is a prerelease dependency (1.16.0-preview). Warning only, does not block NuGet upload. Same as 4.0.0.
- **NuGet.org does not allow re-pushing same version** — if you need to fix a package, you must bump the version number
- **Apostrophe in `A Doll's House.stbx`** breaks MSBuild's auto-generated contentFiles props when consuming the NuGet package. Workaround: `ExcludeAssets="contentFiles"` on the PackageReference. All assets are embedded resources in the DLL, so contentFiles are redundant.

---

## Outliner Migration to NuGet (2026-04-01 — 04-02, in progress)

### Branch: `issue-1246-repo-reorg` in StoryCADAPI repo

### Completed
- [x] Switched ProjectReference → `<PackageReference Include="StoryCADLib" Version="4.0.2" ExcludeAssets="contentFiles" />`
- [x] Removed StoryCADLib content workaround targets from both Outliner.csproj and OutlinerTests.csproj
- [x] Fixed namespace references: `StoryCAD.*` → `StoryCADLib.*` (all source + test files)
- [x] Fixed DI init: replaced manual `services.AddSingleton<StoryCADApi>()` with `BootStrapper.Initialise(headless: true)` in tests
- [x] Fixed App.xaml.cs: `BootStrapper.Initialise(headless: false)` for the app (not headless — it has a UI)
- [x] Wired up Semantic Kernel: reads `OPENAI_API_KEY` env var, model from `OPENAI_MODEL` env var (defaults to `gpt-4o`)
- [x] Rewrote OutlineBuilderTests as integration tests against real API (no mocking)
- [x] Fixed ProseAnalyzerTests: use `Kernel.CreateBuilder().Build()` instead of `Mock<Kernel>()` (Kernel is sealed)
- [x] Fixed window handle bug: ContentPage read `App.MWindowHandle` at construction time before it was set; changed to lazy property
- [x] 29/29 tests passing

### Problems encountered during Outliner work

1. **NuGet contentFiles apostrophe bug**: The .nupkg includes `A Doll's House.stbx` as a contentFile. MSBuild generates an `Exists()` condition with the path, and the apostrophe breaks the condition parser. Fix: `ExcludeAssets="contentFiles"` since all assets are embedded resources.

2. **Namespace mismatch**: Outliner source used `using StoryCAD.*` but the DLL namespaces are `StoryCADLib.*`. This worked with ProjectReference (unclear why — possibly different build) but fails with NuGet PackageReference. Agent investigation found the root cause.

3. **DI registration was wrong**: The original App.xaml.cs and TestSetup.cs manually registered `StoryCADApi` without its dependencies (`OutlineService`, `ListData`, `ControlData`, `ToolsData`). Should use `BootStrapper.Initialise()` which registers everything. The tests had been mocking `StoryCADApi` (concrete class) and `Kernel` (sealed class) — both impossible with Moq. Rewrote as integration tests.

4. **App was marked headless**: I mistakenly set `BootStrapper.Initialise(headless: true)` in App.xaml.cs — a WinUI app with a window is not headless. Only the tests should be headless. Fixed to `headless: false`.

5. **SK was never wired up**: The original WIP code never registered `Kernel` or `IChatCompletionService` in DI. Added SK registration using `BootStrapper.Services` before `Initialise()`.

6. **Wrong model ID**: Used `gpt-5.4` based on agent research — incorrect model ID caused "An error occurred while sending the request". Changed to read `OPENAI_MODEL` env var with `gpt-4o` default, matching StoryCADChat pattern. I wasted time chasing env var visibility (suggesting the key wasn't set, suggesting launchSettings.json which would be a security violation) before identifying the real issue was the model ID.

7. **Window handle timing**: `ContentPage` constructor read `App.MWindowHandle` before `OnLaunched` set it. File picker got handle 0 → COM exception. Fixed by making `WindowHandle` a lazy property that reads `App.MWindowHandle` at use time.

8. **VS launch profile**: Default profile was "Outliner (Package)" which requires MSIX packaging (disabled). User switched to "Outliner (Unpackaged)". Also has typo "Outlilner" in launchSettings.json.

### Still open
- [ ] Verify the app actually works end-to-end with a real OpenAI API call
- [ ] The `Create Outline` button enable/disable behavior may need investigation — appeared grayed out after setting output file in one test
- [ ] Fix typo in launchSettings.json ("Outlilner")
- [ ] Update issue #1246 checkbox

---

## IStoryCADAPI interface gap
- `AddElement` (both overloads at lines 484, 564 of StoryCADAPI.cs) is missing from `IStoryCADAPI` interface
- `OutlineBuilder` uses `AddElement` extensively — cannot program to the interface until this is added
- Needs a separate issue or inclusion in an existing one

## Future work (for AutoBuilder integration)
Jake has an issue open to add NuGet publish to the CI/CD AutoBuilder release workflow. When that happens:

- **Remove dead dependency**: `Microsoft.SemanticKernel.Planners.OpenAI` is in StoryCADLib.csproj but not referenced in any source code. Deprecated, preview-only. Removing it eliminates NU5104 warning.
- **Version management**: csproj has hardcoded version fields. AutoBuilder passes versions via `-p:Version=X.Y.Z`. The `build-release.yml` workflow already does this. NuGet pack step needs `-p:IsPackable=true`.
- **ContentFiles fix**: Should add `<Pack>false</Pack>` to the EmbeddedResource items in StoryCADLib.csproj so the redundant contentFiles aren't included in the .nupkg. This would eliminate the apostrophe bug and remove the need for `ExcludeAssets="contentFiles"` on consumer PackageReferences.
