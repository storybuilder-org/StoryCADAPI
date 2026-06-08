---
layout: default
title: "Search Operations"
parent: "Operations"
nav_order: 1
---

# Search Operations

The API provides three search methods for finding elements by text content or by GUID reference, plus a method for cleaning up references when removing elements.

## SearchForText

Searches all text fields of every element for a case-insensitive match.

<div class="code-tabs" markdown="1">

```csharp
var result = api.SearchForText("treasure");
if (result.IsSuccess)
{
    foreach (var match in result.Payload)
    {
        Console.WriteLine($"{match["Type"]}: {match["Name"]} ({match["Guid"]})");
    }
}
```

```python
for match in sc.search_for_text("treasure"):
    print(f"{match['Type']}: {match['Name']} ({match['Guid']})")
```

</div>

**Returns**: A list of dictionaries, each containing:

| Key | Type | Description |
|-----|------|-------------|
| `Guid` | `Guid` | Element's unique identifier |
| `Name` | `string` | Element's display name |
| `Type` | `string` | Element type (e.g., "Scene", "Character") |

The search covers all `[JsonInclude]` string properties: names, descriptions, notes, goals, conflicts, and any other text field on any element type.

## SearchForReferences

Finds all elements that contain a specific GUID in any of their properties. This is the inverse of reading a GUID property: instead of asking "what Setting does this Scene use?", you ask "which Scenes use this Setting?"

<div class="code-tabs" markdown="1">

```csharp
// Find every element that references a specific character
var characterGuid = /* a Character element's GUID */;
var result = api.SearchForReferences(characterGuid);

foreach (var match in result.Payload)
{
    Console.WriteLine($"{match["Type"]}: {match["Name"]}");
}
// Output might include:
//   Problem: "The Central Conflict"    (character is Protagonist)
//   Scene: "Opening"                   (character is in CastMembers)
//   Scene: "Climax"                    (character is ViewpointCharacter)
//   Character: "Jordan"                (character is in RelationshipList)
```

```python
# Find every element that references a specific character
character = ...  # a Character element handle
for match in sc.search_for_references(character):
    print(f"{match['Type']}: {match['Name']}")
# Output might include:
#   Problem: "The Central Conflict"    (character is Protagonist)
#   Scene: "Opening"                   (character is in CastMembers)
#   Scene: "Climax"                    (character is ViewpointCharacter)
#   Character: "Jordan"                (character is in RelationshipList)
```

</div>

**Returns**: Same dictionary structure as `SearchForText`.

### What Counts as a Reference?

Any property that stores a GUID pointing to another element:

| Element Type | GUID Properties |
|-------------|-----------------|
| Problem | `Protagonist`, `Antagonist` |
| Scene | `ViewpointCharacter`, `Setting`, `Protagonist`, `Antagonist`, `CastMembers` (list) |
| Overview | `StoryProblem`, `ViewpointCharacter` |
| Character | `RelationshipList` entries (each has a partner GUID) |

### Practical Patterns

**Find orphan elements** (elements not referenced by anything):

<div class="code-tabs" markdown="1">

```csharp
var characters = api.GetElementsByType(StoryItemType.Character).Payload;
foreach (var character in characters)
{
    var refs = api.SearchForReferences(character.Uuid).Payload;
    if (refs.Count == 0)
    {
        Console.WriteLine($"Orphan: {character.Name}, not referenced by any scene or problem");
    }
}
```

```python
for character in sc.get_elements_by_type(sc.item_type.Character):
    refs = sc.search_for_references(character)
    if len(refs) == 0:
        print(f"Orphan: {character.name}, not referenced by any scene or problem")
```

</div>

**Build a dependency graph** (who depends on whom):

<div class="code-tabs" markdown="1">

```csharp
var settings = api.GetElementsByType(StoryItemType.Setting).Payload;
foreach (var setting in settings)
{
    var refs = api.SearchForReferences(setting.Uuid).Payload;
    Console.WriteLine($"Setting '{setting.Name}' used by {refs.Count} scene(s):");
    foreach (var scene in refs.Where(r => r["Type"].ToString() == "Scene"))
    {
        Console.WriteLine($"  - {scene["Name"]}");
    }
}
```

```python
for setting in sc.get_elements_by_type(sc.item_type.Setting):
    refs = sc.search_for_references(setting)
    print(f"Setting '{setting.name}' used by {len(refs)} scene(s):")
    for scene in (r for r in refs if r["Type"] == "Scene"):
        print(f"  - {scene['Name']}")
```

</div>

## SearchInSubtree

Searches for text only within a specific branch of the tree. Useful for scoping searches to a folder or section.

<div class="code-tabs" markdown="1">

```csharp
// Search only within "Act 1" folder
var folderGuid = /* GUID of a Folder element */;
var result = api.SearchInSubtree(folderGuid, "conflict");

foreach (var match in result.Payload)
{
    Console.WriteLine($"{match["Type"]}: {match["Name"]}");
}
```

```python
# Search only within "Act 1" folder
folder = ...  # a Folder element handle
for match in sc.search_in_subtree(folder, "conflict"):
    print(f"{match['Type']}: {match['Name']}")
```

</div>

**Parameters**:
- `rootNodeGuid`: the GUID of the element whose subtree to search
- `searchText`: case-insensitive text to find

**Returns**: Same dictionary structure as `SearchForText`, but only elements that are descendants of the specified root node.

This is useful when an outline has many elements and you want to narrow results. For example, searching within a specific chapter's Section node in the Narrator View.

## RemoveReferences

Clears all GUID references to a specified element from the entire model. This is a cleanup operation. Use it when you want to ensure no dangling references remain after conceptually removing an element.

<div class="code-tabs" markdown="1">

```csharp
// Remove all references to a character before deleting
var result = api.RemoveReferences(characterGuid);
if (result.IsSuccess)
{
    Console.WriteLine($"Cleaned {result.Payload} element(s)");
}

// Now safe to delete
await api.DeleteElement(characterGuid);
```

```python
# Remove all references to a character before deleting
# (the wrapper returns the count directly and raises on error)
cleaned = sc.remove_references(character)
print(f"Cleaned {cleaned} element(s)")

# Now safe to delete
sc.delete_element(character)
```

</div>

**Returns**: An `int`: the number of elements that had references removed.

> **Note**: `DeleteElement` moves an element to the trash but does **not** automatically clean up references from other elements. If you want a clean deletion, call `RemoveReferences` first, then `DeleteElement`. Alternatively, leave the references in place: they'll point to `Guid.Empty` (unresolved) once the element is trashed.

## Method Summary

| Method | Input | Returns | Modifies Model? |
|--------|-------|---------|-----------------|
| `SearchForText(text)` | Search string | Matching elements | No |
| `SearchForReferences(guid)` | Target GUID | Elements referencing it | No |
| `SearchInSubtree(rootGuid, text)` | Root + search string | Matches in subtree | No |
| `RemoveReferences(guid)` | Target GUID | Count of cleaned elements | **Yes** |
