# Story Metrics

Builds a richer story outline and computes analytics from it, displaying a formatted dashboard of story metrics. This sample shows how to use the query and inspection APIs to extract meaningful statistics from an outline.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples/StoryMetrics)

## What It Demonstrates

The sample creates a mystery outline called "Murder at Whitmore Manor" with 2 Problems, 4 Characters, 3 Settings, and 6 Scenes, all linked together with cast assignments, settings, and protagonist/antagonist roles.

It then computes and displays five categories of analytics:

- **Element counts by type** — how many Characters, Scenes, Problems, Settings, and Folders the outline contains
- **Character appearances** — using `SearchForReferences` to find every scene and problem that references each character
- **Setting usage** — how many scenes use each setting
- **Problem scope** — whether each problem has a protagonist and antagonist assigned
- **Summary statistics** — totals across all categories

This demonstrates how to combine `GetElementsByType`, `GetStoryElement`, and `SearchForReferences` to build analytical tools on top of the story graph.

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with initial property values |
| `UpdateElementProperties` | Set Setting GUIDs, Protagonist/Antagonist |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | Count total elements |
| `GetElementsByType` | Count and list elements per type |
| `GetStoryElement` | Retrieve typed model (e.g., `ProblemModel`) for property inspection |
| `SearchForReferences` | Find all elements referencing a given UUID |

## How to Run

```bash
cd samples/StoryMetrics
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```
