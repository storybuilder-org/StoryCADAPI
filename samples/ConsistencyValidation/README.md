# ConsistencyValidation

Creates an outline with intentional issues and runs validation checks, printing a quality report.

## What It Does

1. Creates an outline with deliberate problems:
   - An orphan character (never referenced)
   - An unused setting (never linked to a scene)
   - A problem without a protagonist
   - Scenes missing conflict, outcome, or setting
2. Runs 6 validation checks against the outline
3. Prints a formatted report of all issues found

## Validation Checks

| # | Check | What It Detects |
|---|-------|----------------|
| 1 | Scenes without conflict | No Protagonist AND no Antagonist assigned |
| 2 | Scenes without outcome | Empty Outcome field |
| 3 | Problems without protagonist | No Protagonist assigned |
| 4 | Orphan characters | Never referenced by any scene or problem |
| 5 | Unused settings | Never referenced by any scene |
| 6 | Scenes without setting | No Setting assigned |

## How to Run

```bash
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with properties |
| `UpdateElementProperties` | Set properties on elements |
| `AddCastMember` | Link characters to scenes |
| `GetElementsByType` | Get all elements of a given type |
| `GetStoryElement` | Retrieve typed model (SceneModel, ProblemModel) for property inspection |
| `SearchForReferences` | Find elements referencing a UUID (for orphan/unused detection) |

## Prerequisites

- .NET 10.0 SDK
- StoryCADLib project reference (relative path: `../../StoryCAD/StoryCADLib/StoryCADLib.csproj`)
