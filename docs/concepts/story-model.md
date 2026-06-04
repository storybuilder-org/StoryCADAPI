---
layout: default
title: "The Story Model"
parent: "Concepts"
nav_order: 1
---

# The Story Model

The `StoryModel` class is the root data structure for every StoryCAD outline. Understanding how it organizes elements is essential for working with the API.

## Overview

A `StoryModel` contains three things:

1. **A flat element collection** (`StoryElementCollection`) — every story element stored in a dictionary keyed by GUID
2. **Two tree views** — `ExplorerView` (for authoring) and `NarratorView` (for reading order)
3. **A trash view** — `TrashView` for soft-deleted elements

```
StoryModel
├── StoryElements          ← flat dictionary of all elements (keyed by GUID)
├── ExplorerView           ← tree: Overview → Problems, Characters, Scenes, Settings, Folders
├── NarratorView           ← tree: Sections containing Scenes in reading order
└── TrashView              ← tree: soft-deleted elements
```

## The Element Collection

`StoryElementCollection` extends `ObservableCollection<StoryElement>` and automatically maintains:

- `StoryElementGuids` — a `Dictionary<Guid, StoryElement>` for O(1) lookup by GUID
- `Characters`, `Settings`, `Problems`, `Scenes` — filtered sub-collections by type

When you call `GetAllElements()`, you receive the full `StoryElementCollection`. When you call `GetElement(guid)`, the API looks up the element in `StoryElementGuids`.

<div class="code-tabs" markdown="1">

```csharp
// Get every element
var all = api.GetAllElements().Payload;

// Get one element by GUID
var element = api.GetElement(someGuid).Payload;

// Get all elements of a type
var characters = api.GetElementsByType(StoryItemType.Character).Payload;
```

```python
# Get every element
all_elements = sc.get_all_elements()

# Get one element by handle (returns its full state as a JSON string)
element = sc.get_element(some_element)

# Get all elements of a type
characters = sc.get_elements_by_type(sc.item_type.Character)
```

</div>

## GUID-Based Cross-Referencing

Elements reference each other through GUIDs, not object references. This is the most important concept for API consumers.

For example, a `ProblemModel` has:

<div class="code-tabs" markdown="1">

```csharp
public Guid Protagonist { get; set; }   // GUID of a CharacterModel
public Guid Antagonist { get; set; }     // GUID of a CharacterModel
```

```python
# A Problem's reference fields are GUID strings (C#-type concept).
# Read them from the element's JSON state:
detail = json.loads(sc.get_element(problem))
protagonist_guid = detail.get("Protagonist")  # GUID of a Character
antagonist_guid = detail.get("Antagonist")    # GUID of a Character
```

</div>

A `SceneModel` has:

<div class="code-tabs" markdown="1">

```csharp
public Guid Setting { get; set; }              // GUID of a SettingModel
public Guid ViewpointCharacter { get; set; }   // GUID of a CharacterModel
public Guid Protagonist { get; set; }          // GUID of a CharacterModel
public Guid Antagonist { get; set; }           // GUID of a CharacterModel
public List<Guid> CastMembers { get; set; }    // GUIDs of CharacterModels
```

```python
# A Scene's reference fields are GUID strings (C#-type concept).
# Read them from the element's JSON state:
detail = json.loads(sc.get_element(scene))
setting_guid = detail.get("Setting")                # GUID of a Setting
viewpoint_guid = detail.get("ViewpointCharacter")   # GUID of a Character
protagonist_guid = detail.get("Protagonist")        # GUID of a Character
antagonist_guid = detail.get("Antagonist")          # GUID of a Character
cast_member_guids = detail.get("CastMembers")       # list of Character GUIDs
```

</div>

**Why GUIDs instead of object references?** The GUID-based design enables:

- JSON serialization without circular references
- Stable references that survive save/load cycles
- Multiple views of the same element (ExplorerView and NarratorView can reference the same Scene)
- Safe deletion (removing an element doesn't break the object graph)

An unassigned reference uses `Guid.Empty`. For example, a new Scene's `Setting` property is `Guid.Empty` until a Setting is assigned.

### Resolving References

To follow a GUID reference, look up the target element:

<div class="code-tabs" markdown="1">

```csharp
// Get a scene
var sceneResult = api.GetElement(sceneGuid);
// The scene's Setting field is a GUID string — parse it and look up the setting
var settingGuid = /* extract from scene data */;
var settingResult = api.GetElement(settingGuid);
```

```python
# Get a scene's full state as JSON
scene_data = json.loads(sc.get_element(scene))
# The scene's Setting field is a GUID string — extract it and look up the setting
setting_guid = scene_data.get("Setting")
setting_data = json.loads(sc.get_element(setting_guid))
```

</div>

To find all elements that reference a given element, use `SearchForReferences`:

<div class="code-tabs" markdown="1">

```csharp
// Find everything that references this character
var refs = api.SearchForReferences(characterGuid).Payload;
// Returns: scenes where they appear, problems where they're protagonist/antagonist, etc.
```

```python
# Find everything that references this character
refs = sc.search_for_references(character)
# Returns: scenes where they appear, problems where they're protagonist/antagonist, etc.
```

</div>

## The Explorer View

The `ExplorerView` is the authoring tree. Its root is always the `StoryOverview` element (the story's title node). All other elements are children or descendants of the overview.

```
Overview: "My Novel"
├── Problem: "The Central Conflict"
├── Character: "Alex"
├── Character: "Jordan"
├── Folder: "Act 1"
│   ├── Scene: "Opening"
│   └── Scene: "Inciting Incident"
├── Setting: "The City"
└── StoryWorld: "Story World"
```

Rules:
- The overview is always the first (root) node
- Folders organize elements in the Explorer View
- Any element type except Section can be a child of the overview or a folder
- The TrashCan is a separate root node in `TrashView`

## The Narrator View

The `NarratorView` organizes scenes in reading order using `Section` elements (chapters, acts, parts):

```
Section: "Part 1"
├── Section: "Chapter 1"
│   ├── Scene: "Opening"
│   └── Scene: "Inciting Incident"
└── Section: "Chapter 2"
    └── Scene: "Rising Action"
```

Rules:
- Only Section and Scene elements belong in the Narrator View
- Sections can nest (Part → Chapter → Scene)
- Scenes in the Narrator View reference the same `StoryElement` as in the Explorer View

## Singleton Elements

Three element types have at most one instance per story:

| Type | Purpose | Always present? |
|------|---------|----------------|
| `StoryOverview` | Story metadata (title, author, premise) | Yes — root of ExplorerView |
| `TrashCan` | Container for soft-deleted elements | Yes — root of TrashView |
| `StoryWorld` | Worldbuilding data | No — optional, created on demand |

Access the overview via `GetElementsByType(StoryItemType.StoryOverview)`. Access the StoryWorld via `GetStoryWorld()`.

## The Changed Flag

`StoryModel.Changed` is a dirty bit. It becomes `true` whenever an element is added, removed, moved, or has a property updated. The API sets this automatically — you don't need to manage it. It's used internally by auto-save.

## File Format

Story outlines are saved as `.stbx` files (JSON). The serialized format contains:

- All `StoryElement` instances (polymorphic, with type discriminator)
- Flattened tree structures (`FlattenedExplorerView`, `FlattenedNarratorView`, `FlattenedTrashView`) as lists of `(Uuid, ParentUuid)` pairs
- Version metadata (`CreatedVersion`, `LastVersion`)

The tree views are reconstructed from the flattened lists on load.

<div class="code-tabs" markdown="1">

```csharp
// Save
await api.WriteOutline("my-story.stbx");

// Load
await api.OpenOutline("my-story.stbx");
```

```python
# Save
sc.write_outline("my-story.stbx")

# Load
sc.open_outline("my-story.stbx")
```

</div>
