# Story Graph Basics

The foundational walkthrough of the StoryCADLib API. This sample takes you through the complete lifecycle of a story outline: creating it, populating it with elements, linking those elements together, saving to disk, and reloading to verify the round-trip.

> [!TIP]
> If you haven't already, run through the [Quick Start](../getting-started/quick-start.md) first — it's a copy-paste Hello World that gets you up and running in five minutes. Once that works, come back here. Every other sample builds on the patterns introduced in this one.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples/StoryGraphBasics)

## What It Demonstrates

The sample creates an outline called "The Clockwork Conspiracy" and populates it with six story elements: one Problem, two Characters, one Setting, and two Scenes. It then links them together — assigning a Protagonist and Antagonist to the Problem, attaching a Setting and Cast to each Scene, and creating a mirrored relationship between the two characters.

After building the outline, it queries the story graph three ways: listing all elements, filtering by type, and retrieving a single element by GUID. Finally, it writes the outline to a `.stbx` file, reopens it, and confirms the element count matches.

This covers the core create-link-query-persist workflow that every StoryCADLib consumer needs.

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add Problem, Character, Setting, Scene |
| `UpdateElementProperties` | Set Protagonist, Antagonist, Setting GUIDs |
| `AddCastMember` | Link characters to scenes |
| `AddRelationship` | Create character-to-character relationship |
| `GetAllElements` | List every element in the model |
| `GetElementsByType` | Filter elements by StoryItemType |
| `GetElement` | Retrieve a single element's full data |
| `WriteOutline` | Save model to .stbx file |
| `OpenOutline` | Load model from .stbx file |

## How to Run

```bash
cd samples/StoryGraphBasics
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```
