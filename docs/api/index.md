---
layout: default
title: API Reference
nav_order: 8
has_children: true
---

# API Reference

This section documents the StoryCADLib public API.

## Core Classes

| Class | Description |
|-------|-------------|
| [StoryCADApi](storycad-api.md) | Main API for story outline operations |
| [OperationResult\<T\>](operation-result.md) | Result wrapper for all API calls |

## Namespaces

Generated reference for the supporting model types:

| Namespace | Description |
|-----------|-------------|
| [Models](models.md) | Core story element and document models |
| [Models.Tools](models-tools.md) | Writing-tool data models (conflicts, key questions, plot patterns) |
| [Models.Scrivener](models-scrivener.md) | Scrivener import/export models |
| [Models.StoryWorld](models-storyworld.md) | Story world-building entry models |

## Quick Reference by Category

### Outline Operations
| Method | Description |
|--------|-------------|
| `CreateEmptyOutline` | Create a new story outline |
| `OpenOutline` | Open an existing .stbx file |
| `WriteOutline` | Save the outline to disk |

### Element Operations
| Method | Description |
|--------|-------------|
| `GetAllElements` | Get all elements in the model |
| `GetElementsByType` | Get elements of a specific type |
| `GetStoryElement` | Get a specific element by GUID |
| `AddElement` | Create a new story element |
| `AddElementWithProperties` | Create element with initial properties and optional GUID override |
| `UpdateElementProperty` | Update a single property |
| `UpdateElementProperties` | Update multiple properties |
| `DeleteElement` | Move element to trash |
| `GetElement` | Get element with all fields as serialized object |
| `UpdateStoryElement` | Replace an entire element |
| `MoveElement` | Move element to a new parent in the outline tree |

### Search Operations
| Method | Description |
|--------|-------------|
| `SearchForText` | Full-text search across all elements |
| `SearchForReferences` | Find elements referencing a target |
| `SearchInSubtree` | Search within a subtree |
| `RemoveReferences` | Remove all references to an element |

### Internal / Advanced
| Method | Description |
|--------|-------------|
| `SetCurrentModel` | Set the active StoryModel (used by Collaborator) |
| `DeleteStoryElement` | Move element to trash (string GUID variant) |

### Relationship Operations
| Method | Description |
|--------|-------------|
| `AddCastMember` | Add character to scene cast |
| `AddRelationship` | Create relationship between characters |

### Trash Operations
| Method | Description |
|--------|-------------|
| `RestoreFromTrash` | Restore deleted element |
| `EmptyTrash` | Permanently delete trashed items |

### Resource Data
| Method | Description |
|--------|-------------|
| `GetExamples` | Get example values for properties |
| `GetConflictCategories` | Get conflict type categories |
| `GetKeyQuestionElements` | Get key question prompts |

## Element Types

The API works with these story element types:

| Type | Description |
|------|-------------|
| `StoryOverview` | Root element with story metadata (singleton) |
| `Problem` | Story problems, conflicts, themes |
| `Character` | Characters in the story |
| `Scene` | Individual scenes |
| `Setting` | Locations and places |
| `Folder` | Organizational container |
| `Section` | Grouping for scenes (acts, chapters) |
| `Web` | Web links and references |
| `Notes` | Freeform notes |
| `TrashCan` | Deleted elements (singleton) |
