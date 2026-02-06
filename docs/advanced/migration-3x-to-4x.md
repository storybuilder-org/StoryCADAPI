# Migration Guide: 3.x to 4.x

This guide covers breaking changes when upgrading from StoryCADLib 3.x to 4.x.

## Breaking Changes Summary

| Area | 3.x | 4.x | Action |
|------|-----|-----|--------|
| Namespace | `StoryCAD.Services.API` | `StoryCADLib.Services.API` | Update `using` statements |
| Class name | `SemanticKernelAPI` | `SemanticKernelApi` | Rename (lowercase `pi`) |
| Target framework | `net8.0-windows10.0.22621` | `net10.0-desktop` or `net10.0-windows10.0.22621` | Update TFM |
| Platform | Windows only | Windows, macOS, Linux | No code change (but more options) |
| New element type | — | `StoryWorld` | Handle in element iteration |
| IoC namespace | `StoryCAD.Services.IoC` | `StoryCADLib.Services.IoC` | Update `using` statements |

## Namespace Change

The root namespace changed from `StoryCAD` to `StoryCADLib`.

**Before (3.x)**:

```csharp
using StoryCAD.Services.API;
using StoryCAD.Services.IoC;
using StoryCAD.Models;
```

**After (4.x)**:

```csharp
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;
using StoryCADLib.Models;
```

## Class Rename

The API class was renamed for .NET naming conventions.

**Before (3.x)**:

```csharp
var api = Ioc.Default.GetRequiredService<SemanticKernelAPI>();
```

**After (4.x)**:

```csharp
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();
```

## Target Framework

StoryCADLib 4.x requires .NET 10.0 and uses UNO Platform for cross-platform support.

**Before (3.x)**:

```xml
<TargetFramework>net8.0-windows10.0.22621</TargetFramework>
<PackageReference Include="StoryCADLib" Version="3.*" />
```

**After (4.x)**:

```xml
<!-- For cross-platform (Windows, macOS, Linux): -->
<TargetFramework>net10.0-desktop</TargetFramework>

<!-- Or for Windows-only with WinAppSDK: -->
<TargetFramework>net10.0-windows10.0.22621</TargetFramework>

<PackageReference Include="StoryCADLib" Version="4.*" />
```

The `net10.0-desktop` target produces binaries that run on Windows, macOS, and Linux. Use this for cross-platform console apps and services.

## New Element Type: StoryWorld

4.x adds `StoryItemType.StoryWorld` to the enum. If your code iterates over element types or uses a switch statement, add a case for `StoryWorld`.

**Before (3.x)**:

```csharp
switch (element.ElementType)
{
    case StoryItemType.Character: /* ... */ break;
    case StoryItemType.Scene:     /* ... */ break;
    case StoryItemType.Problem:   /* ... */ break;
    case StoryItemType.Setting:   /* ... */ break;
    // ...
}
```

**After (4.x)**:

```csharp
switch (element.ElementType)
{
    case StoryItemType.Character:   /* ... */ break;
    case StoryItemType.Scene:       /* ... */ break;
    case StoryItemType.Problem:     /* ... */ break;
    case StoryItemType.Setting:     /* ... */ break;
    case StoryItemType.StoryWorld:  /* ... */ break;  // NEW
    // ...
}
```

`StoryWorld` is an optional singleton (at most one per story). Access it with `GetStoryWorld()`.

## New API Methods in 4.x

4.x adds significant new functionality. These are additive (not breaking) but worth knowing about:

### Beat Sheet API (13 methods)

Apply story structure templates to Problem elements: `GetBeatSheetNames`, `GetBeatSheet`, `ApplyBeatSheetToProblem`, `GetProblemStructure`, `AssignElementToBeat`, `ClearBeatAssignment`, `CreateBeat`, `UpdateBeat`, `DeleteBeat`, `MoveBeat`, `SaveBeatSheet`, `LoadBeatSheet`.

See [Beat Sheet Operations](../operations/beat-sheets.md).

### Resource API (14 methods)

Access built-in writing reference data: `GetExamples`, `GetConflictCategories`, `GetConflictSubcategories`, `GetConflictExamples`, `ApplyConflictToProtagonist`, `ApplyConflictToAntagonist`, `GetKeyQuestionElements`, `GetKeyQuestions`, `GetMasterPlotNames`, `GetMasterPlotNotes`, `GetMasterPlotScenes`, `GetStockSceneCategories`, `GetStockScenes`.

See [Resource Workflows](../operations/resource-workflows.md).

### Search API (4 methods)

Find elements by text or by GUID reference: `SearchForText`, `SearchForReferences`, `SearchInSubtree`, `RemoveReferences`.

See [Search Operations](../operations/search.md).

### Trash Management (2 methods)

Soft delete and restore: `RestoreFromTrash`, `EmptyTrash`.

### Element Convenience Methods

- `GetStoryWorld()` — direct access to the StoryWorld singleton
- `GetElementsByType(type)` — filter elements by type
- `AddElement` overload with `Dictionary<string, object> properties` — create and populate in one call

## File Format Compatibility

4.x `.stbx` files may contain `StoryWorld` elements that 3.x cannot read. Files created by 3.x are fully compatible with 4.x — they open without modification.

If you need to share files between 3.x and 4.x users, avoid adding `StoryWorld` elements to the outline.

## Migration Checklist

- [ ] Update all `using StoryCAD.` statements to `using StoryCADLib.`
- [ ] Rename `SemanticKernelAPI` to `SemanticKernelApi`
- [ ] Update `.csproj` target framework to `net10.0-desktop` or `net10.0-windows10.0.22621`
- [ ] Update `PackageReference` to `StoryCADLib` version `4.*`
- [ ] Add `StoryItemType.StoryWorld` case to any element type switches
- [ ] Test with existing `.stbx` files to verify compatibility
- [ ] Review new API methods for features that simplify your code
