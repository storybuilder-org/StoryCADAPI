# Consistency Validation

Creates an outline with intentional structural problems and runs six validation checks against it, printing a quality report. This sample demonstrates how to build automated validation tools using the StoryCADLib query APIs.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/ConsistencyValidation)

## What It Demonstrates

The sample builds an outline that contains seven deliberate issues: an orphan character (never referenced by any scene or problem), an unused setting (never linked to a scene), a problem without a protagonist, and scenes missing conflict, outcome, or setting assignments.

It then runs six validation checks and prints a formatted report listing every issue found:

| Check | What It Detects |
|-------|----------------|
| Scenes without conflict | No Protagonist AND no Antagonist assigned |
| Scenes without outcome | Empty Outcome field |
| Problems without protagonist | No Protagonist assigned |
| Orphan characters | Never referenced by any scene or problem |
| Unused settings | Never referenced by any scene |
| Scenes without setting | No Setting assigned |

The validation logic combines `GetElementsByType` to enumerate elements, `GetStoryElement` to inspect typed properties (like `SceneModel.Outcome` or `ProblemModel.Protagonist`), and `SearchForReferences` to detect orphaned elements. This pattern is the foundation for building linting or quality-gate tools on top of StoryCADLib.

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with properties |
| `UpdateElementProperties` | Set properties on elements |
| `AddCastMember` | Link characters to scenes |
| `GetElementsByType` | Get all elements of a given type |
| `GetStoryElement` | Retrieve typed model (`SceneModel`, `ProblemModel`) for property inspection |
| `SearchForReferences` | Find elements referencing a UUID (for orphan/unused detection) |

## How to Run

```bash
cd ConsistencyValidation
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```
