# StoryGraphBasics

A foundational "hello world" sample for the StoryCADLib API. Creates a story outline, populates it with elements, links them together, saves to disk, and reloads to verify.

## What It Does

1. Initializes StoryCADLib in headless mode (no UI)
2. Creates an empty outline ("The Clockwork Conspiracy")
3. Adds story elements: 1 Problem, 2 Characters, 1 Setting, 2 Scenes
4. Links elements: sets Protagonist/Antagonist on the Problem, assigns Setting and Cast on Scenes
5. Creates a mirrored relationship between characters
6. Queries the outline: GetAllElements, GetElementsByType, GetElement
7. Saves to a `.stbx` file, reopens it, and verifies the element count matches

## How to Run

```bash
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

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

## Prerequisites

- .NET 10.0 SDK
- StoryCADLib project reference (relative path: `../../StoryCAD/StoryCADLib/StoryCADLib.csproj`)
