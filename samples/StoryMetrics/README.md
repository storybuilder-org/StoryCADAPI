# StoryMetrics

Builds a richer story outline and computes analytics, displaying a formatted dashboard of story metrics.

## What It Does

1. Creates a mystery outline ("Murder at Whitmore Manor") with 2 Problems, 4 Characters, 3 Settings, and 6 Scenes
2. Links elements together (cast members, settings, protagonist/antagonist assignments)
3. Computes and displays analytics:
   - Element counts by type
   - Character appearances (via `SearchForReferences`)
   - Setting usage frequency
   - Problem scope (protagonist/antagonist assignment)
   - Summary statistics

## How to Run

```bash
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` (with properties) | Add elements with initial property values |
| `UpdateElementProperties` | Set Setting GUIDs on scenes, Protagonist/Antagonist on problems |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | Count total elements |
| `GetElementsByType` | Count and list elements per type |
| `GetStoryElement` | Retrieve typed model (e.g., ProblemModel) for property inspection |
| `SearchForReferences` | Find all elements referencing a given UUID |

## Prerequisites

- .NET 10.0 SDK
- StoryCADLib project reference (relative path: `../../StoryCAD/StoryCADLib/StoryCADLib.csproj`)
