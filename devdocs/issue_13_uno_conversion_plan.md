# Issue #13 — Outliner Uno Conversion Plan

Branch: `issue-13-convert-to-uno-app`

## Context

Outliner is currently a pure Windows WinUI 3 app (`Microsoft.NET.Sdk`, `net10.0-windows10.0.22621`, `Microsoft.WindowsAppSDK`). To make it useful in classroom/education settings and as a sales tool for StoryCAD on macOS, it needs to build and run on both Windows and macOS — the same goal that motivated switching StoryCADCritter to `Uno.Sdk`.

The conversion switches to `Uno.Sdk` with OS-conditioned multi-targeting: on Windows both `net10.0-windows10.0.22621` (WinAppSDK/native WinUI 3) and `net10.0-desktop` (Skia) are built; on macOS only `net10.0-desktop` (Skia). This matches the pattern used in `StoryCAD/StoryCAD/StoryCAD.csproj`. When running, you specify which TFM to use (`-f net10.0-windows10.0.22621` or `-f net10.0-desktop`).

StoryCADLib 4.1.1 already ships both `net10.0-desktop` and `net10.0-windows` assets — no upstream blocker. `global.json` already pins `Uno.Sdk 6.5.31`.

## Windows-Specific Code Identified

Four files contain `WinRT.Interop` calls that need `#if WINDOWS` guards:

| File | What needs guarding |
|------|---|
| `App.xaml.cs` | `WindowNative.GetWindowHandle(MWindow)` — sets `MWindowHandle` |
| `ContentPageViewModel.cs` | `FileOpenPicker` + `FileSavePicker` init (2 sites) |
| `BatchPageViewModel.cs` | `FolderPicker` init (2 sites) |
| `SettingsPageViewModel.cs` | `FolderPicker` init (1 site) |

No `#if WINDOWS` guards exist today. All XAML files are unaffected — WinUI XAML is Uno-compatible.

## Implementation Steps

### Step 1 — Outliner.csproj
- `Sdk="Microsoft.NET.Sdk"` → `Sdk="Uno.Sdk"`
- Add `<UnoSingleProject>true</UnoSingleProject>`
- Replace single `<TargetFramework>` with OS-conditioned `<TargetFrameworks>` (pattern from `StoryCAD/StoryCAD/StoryCAD.csproj`):
  ```xml
  <!-- Windows: build both WinAppSDK and Skia Desktop -->
  <PropertyGroup Condition="'$(OS)'=='Windows_NT'">
      <TargetFrameworks>net10.0-windows10.0.22621;net10.0-desktop</TargetFrameworks>
  </PropertyGroup>
  <!-- macOS/Linux: Skia Desktop only -->
  <PropertyGroup Condition="'$(OS)'!='Windows_NT'">
      <TargetFrameworks>net10.0-desktop</TargetFrameworks>
  </PropertyGroup>
  ```
- Condition Windows-only properties on TFM:
  ```xml
  <UseWinUI Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">true</UseWinUI>
  <WindowsAppSDKSelfContained Condition="...== 'windows'">true</WindowsAppSDKSelfContained>
  <EnableMsixTooling Condition="...== 'windows'">false</EnableMsixTooling>
  ```
- Remove: `TargetPlatformMinVersion`, `RuntimeIdentifiers`, flat `Platforms` property
- Keep all packages; `Microsoft.WindowsAppSDK` and `Microsoft.Windows.SDK.BuildTools` are Windows-only by nature
- Add `<UnoFeatures>SkiaRenderer;</UnoFeatures>`
- Condition Windows asset items (scaled logos, `app.manifest`) on TFM

### Step 2 — OutlinerTests.csproj
- Same SDK switch and OS-conditioned TFM PropertyGroups
- Condition `UseWinUI`, `WindowsAppSDKSelfContained` on TFM
- Remove flat `TargetPlatformIdentifier`, `TargetPlatformVersion`, `TargetPlatformMinVersion`
- `OutputType` from `WinExe` → `Exe`
- Keep MSTest packages and project reference to Outliner

### Step 3 — Platform guards in App.xaml.cs
Wrap the window-handle assignment:
```csharp
#if WINDOWS
MWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MWindow);
#endif
```
Guard the `MWindowHandle` field declaration similarly, or make it `IntPtr.Zero` on non-Windows.

### Step 4 — Platform guards in ContentPageViewModel.cs (2 sites)
```csharp
var picker = new FileOpenPicker();
#if WINDOWS
WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MWindowHandle);
#endif
```
Same pattern for `FileSavePicker`.

### Step 5 — Platform guards in BatchPageViewModel.cs (2 sites)
Same `#if WINDOWS` pattern around both `FolderPicker` + `InitializeWithWindow` calls.

### Step 6 — Platform guards in SettingsPageViewModel.cs (1 site)
Same pattern.

### Step 7 — Outliner.sln
Update build configurations: remove per-platform (x86/x64/ARM64) Windows configs; add `Any CPU` config for `net10.0-desktop`.

## What Does NOT Change

- All XAML files — `Microsoft.UI.Xaml` namespace is Uno-compatible
- All service/pipeline code (`OutlineRunner`, `ProseAnalyzer`, `OutlineBuilder`, `ProseDocumentReader`)
- All test logic and fixtures
- `Directory.Build.props` and `global.json`

## Verification

```powershell
# Build
dotnet build samples/Outliner/Outliner/Outliner.csproj -f net10.0-desktop

# Run
dotnet run --project samples/Outliner/Outliner/Outliner.csproj -f net10.0-desktop

# Tests (all 59 must pass)
dotnet test samples/Outliner/OutlinerTests/OutlinerTests.csproj -f net10.0-desktop
```

App must launch, navigate between Single/Batch/Settings pages, and file pickers must work on Windows before declaring Windows success. macOS verification requires a Mac build.
