---
layout: default
title: "StoryCADApi"
parent: "API Reference"
nav_order: 1
---

# StoryCADApi Class

The main entry point for StoryCADLib API operations.

<div class="code-tabs" markdown="1">

```csharp
namespace StoryCADLib.Services.API

public class StoryCADApi : IStoryCADAPI
```

```python
# In Python, the API is the StoryCAD object itself.
from storycad import StoryCAD
```

</div>

## Overview

`StoryCADApi` provides methods for creating, reading, updating, and deleting story elements. All methods are decorated with `[KernelFunction]` for Semantic Kernel integration, but can also be called directly.

**Key Concepts:**
- All methods return `OperationResult<T>` - check `IsSuccess` before using `Payload`
- The API maintains a `CurrentModel` property holding the active story
- Set `CurrentModel` via `CreateEmptyOutline()` or `OpenOutline()` before other operations

## Getting an Instance

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize (once at startup)
ServiceLocator.Initialize(headless: true);

// Get the API instance
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

```python
from storycad import StoryCAD

# Initialize (once at startup) — the sc object is the API instance
sc = StoryCAD(headless=True)
```

</div>

---

## Outline Operations

### CreateEmptyOutline

Creates a new empty story outline based on a template.

<div class="code-tabs" markdown="1">

```csharp
public async Task<OperationResult<List<Guid>>> CreateEmptyOutline(
    string name,
    string author,
    string templateIndex)
```

```python
def create_empty_outline(title, author, template_index="0"):
    ...  # returns a list of element handles; [0] is the Story Overview
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `name` | string | Title of the story |
| `author` | string | Author name |
| `templateIndex` | string | Template index: "0" for blank, "1" for basic |

**Returns:** `OperationResult<List<Guid>>` - GUIDs of created elements

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = await api.CreateEmptyOutline("My Story", "Jane Doe", "0");
if (result.IsSuccess)
{
    Console.WriteLine($"Created {result.Payload.Count} elements");
}
```

```python
elements = sc.create_empty_outline("My Story", "Jane Doe", template_index="0")
print(f"Created {len(elements)} elements")
```

</div>

---

### OpenOutline

Opens an existing story outline from disk.

<div class="code-tabs" markdown="1">

```csharp
public async Task<OperationResult<bool>> OpenOutline(string path)
```

```python
def open_outline(path):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `path` | string | Full path to the .stbx file |

**Returns:** `OperationResult<bool>` - true on success

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = await api.OpenOutline("C:/Stories/my-story.stbx");
if (result.IsSuccess)
{
    var elements = api.GetAllElements();
}
```

```python
sc.open_outline("C:/Stories/my-story.stbx")
elements = sc.get_all_elements()
```

</div>

---

### WriteOutline

Saves the current outline to disk.

<div class="code-tabs" markdown="1">

```csharp
public async Task<OperationResult<string>> WriteOutline(string filePath)
```

```python
def write_outline(path):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `filePath` | string | Full path for the .stbx file |

**Returns:** `OperationResult<string>` - Success message

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = await api.WriteOutline("C:/Stories/my-story.stbx");
if (!result.IsSuccess)
{
    Console.WriteLine($"Save failed: {result.ErrorMessage}");
}
```

```python
try:
    sc.write_outline("C:/Stories/my-story.stbx")
except Exception as error:
    print(f"Save failed: {error}")  # the wrapper raises instead of returning a result
```

</div>

---

## Element Operations

### GetAllElements

Returns all elements in the current model.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<ObservableCollection<StoryElement>> GetAllElements()
```

```python
def get_all_elements():
    ...  # iterable of element handles
```

</div>

**Returns:** `OperationResult<ObservableCollection<StoryElement>>` - All story elements

---

### GetElementsByType

Gets all elements of a specific type.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<List<StoryElement>> GetElementsByType(StoryItemType elementType)
```

```python
def get_elements_by_type(item_type):
    ...  # iterable of element handles
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementType` | StoryItemType | The type to filter by |

**Valid Types:** `Problem`, `Character`, `Setting`, `Scene`, `Folder`, `Section`, `Web`, `Notes`, `StoryWorld`

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = api.GetElementsByType(StoryItemType.Character);
if (result.IsSuccess)
{
    foreach (var character in result.Payload)
    {
        Console.WriteLine(character.Name);
    }
}
```

```python
for character in sc.get_elements_by_type(sc.item_type.Character):
    print(character.name)
```

</div>

---

### GetStoryElement

Gets a specific element by its GUID.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<StoryElement> GetStoryElement(Guid guid)
```

```python
def get_element(element):
    ...  # returns the element's full state as a JSON string
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `guid` | Guid | The element's unique identifier |

**Returns:** `OperationResult<StoryElement>` - The requested element

---

### AddElement

Creates a new story element.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<Guid> AddElement(
    StoryItemType typeToAdd,
    string parentGUID,
    string name,
    string description = "")
```

```python
def add_element(item_type, parent, name):
    ...  # returns an element handle
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `typeToAdd` | StoryItemType | Type of element to create |
| `parentGUID` | string | GUID of parent element (as string) |
| `name` | string | Name for the new element |
| `description` | string | Optional description |

**Returns:** `OperationResult<Guid>` - GUID of created element

**Example:**
<div class="code-tabs" markdown="1">

```csharp
// Get the story overview (root) GUID
var overview = api.GetElementsByType(StoryItemType.StoryOverview);
var rootGuid = overview.Payload.First().Uuid.ToString();

// Add a character
var result = api.AddElement(
    StoryItemType.Character,
    rootGuid,
    "Alex",
    "The protagonist");

if (result.IsSuccess)
{
    Console.WriteLine($"Created character with GUID: {result.Payload}");
}
```

```python
# Get the story overview (root)
overview = next(iter(sc.get_elements_by_type(sc.item_type.StoryOverview)))

# Add a character
character = sc.add_element(sc.item_type.Character, overview, "Alex")
print(f"Created character with GUID: {character.uuid}")
```

</div>

---

### AddElementWithProperties

Creates a new story element with initial properties and an optional GUID override.

> **Semantic Kernel name:** This method is registered as `AddElementWithProperties` via `[KernelFunction("AddElementWithProperties")]` to distinguish it from the basic `AddElement` overload in SK plugin registration.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<Guid> AddElement(
    StoryItemType typeToAdd,
    string parentGUID,
    string name,
    Dictionary<string, object> properties,
    string GUIDOverride = "")
```

```python
# Add the element, then set its initial properties.
element = sc.add_element(item_type, parent, name)
sc.update_element_properties(element, {"Role": "Protagonist"})
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `typeToAdd` | StoryItemType | Type of element to create |
| `parentGUID` | string | GUID of parent element (as string) |
| `name` | string | Name for the new element |
| `properties` | Dictionary | Initial property values to set |
| `GUIDOverride` | string | Optional: specify a GUID for the new element (must be unique) |

**Returns:** `OperationResult<Guid>` - GUID of created element

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var props = new Dictionary<string, object>
{
    { "Role", "Protagonist" },
    { "Age", "28" },
    { "Archetype", "The Hero" }
};

var result = api.AddElement(
    StoryItemType.Character,
    rootGuid,
    "Elena",
    props);
```

```python
elena = sc.add_element(sc.item_type.Character, overview, "Elena")
sc.update_element_properties(
    elena,
    {"Role": "Protagonist", "Age": "28", "Archetype": "The Hero"},
)
```

</div>

---

### UpdateElementProperty

Updates a single property on an element.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<StoryElement> UpdateElementProperty(
    Guid elementUuid,
    string propertyName,
    object value)
```

```python
def update_element_property(element, property_name, value):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementUuid` | Guid | The element's GUID |
| `propertyName` | string | Name of the property to update |
| `value` | object | New value for the property |

**Returns:** `OperationResult<StoryElement>` - Updated element

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = api.UpdateElementProperty(
    characterGuid,
    "Role",
    "Protagonist");
```

```python
sc.update_element_property(character, "Role", "Protagonist")
```

</div>

---

### UpdateElementProperties

Updates multiple properties on an element.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> UpdateElementProperties(
    Guid elementGuid,
    Dictionary<string, object> properties)
```

```python
def update_element_properties(element, properties):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementGuid` | Guid | The element's GUID |
| `properties` | Dictionary | Property names and values |

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var props = new Dictionary<string, object>
{
    { "Name", "Alex" },
    { "Role", "Protagonist" },
    { "Age", "32" }
};
api.UpdateElementProperties(characterGuid, props);
```

```python
sc.update_element_properties(
    character,
    {"Name": "Alex", "Role": "Protagonist", "Age": "32"},
)
```

</div>

---

### DeleteElement

Moves an element to the trash.

<div class="code-tabs" markdown="1">

```csharp
public Task<OperationResult<bool>> DeleteElement(Guid elementToDelete)
```

```python
def delete_element(element):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementToDelete` | Guid | The element's GUID |

**Returns:** `OperationResult<bool>` - true on success

---

### GetElement

Returns a single element with all its fields as a serialized object. Unlike `GetStoryElement` which returns a typed `StoryElement`, this returns the full serialized representation including all fields.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<object> GetElement(Guid guid)
```

```python
def get_element(element):
    ...  # returns the full serialized element as a JSON string
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `guid` | Guid | The element's unique identifier |

**Returns:** `OperationResult<object>` - Serialized element with all fields

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = api.GetElement(characterGuid);
if (result.IsSuccess)
{
    Console.WriteLine(result.Payload); // Full serialized element
}
```

```python
import json

detail = json.loads(sc.get_element(character))
print(detail)  # Full serialized element
```

</div>

---

### UpdateStoryElement

Replaces an entire story element by deserializing a new element and updating it in the model. For updating individual properties, prefer `UpdateElementProperty` or `UpdateElementProperties`.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> UpdateStoryElement(object newElement, Guid guid)
```

```python
# Prefer update_element_property / update_element_properties for individual fields.
sc.update_element_properties(element, {"Name": "Alex"})
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `newElement` | object | Serialized element data (will be deserialized via `StoryElement.Deserialize`) |
| `guid` | Guid | The GUID of the element to replace |

**Returns:** `OperationResult<bool>` - true on success

---

### MoveElement

Moves an element to a new parent in the outline's ExplorerView tree. Validates against circular references, moving the root element, and moving the TrashCan.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> MoveElement(Guid elementGuid, Guid newParentGuid)
```

```python
def move_element(element, new_parent, index=None):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementGuid` | Guid | GUID of the element to move |
| `newParentGuid` | Guid | GUID of the new parent element |

**Returns:** `OperationResult<bool>` - true on success

**Validation:**
- Cannot move an element to itself
- Cannot move the root element
- Cannot move the TrashCan
- Cannot move an element to one of its own descendants (circular reference check)

**Example:**
<div class="code-tabs" markdown="1">

```csharp
// Move a scene under a different folder
var result = api.MoveElement(sceneGuid, newFolderGuid);
if (result.IsSuccess)
{
    Console.WriteLine("Element moved successfully");
    await api.WriteOutline("my-story.stbx"); // Save changes
}
```

```python
# Move a scene under a different folder
sc.move_element(scene, new_folder)
print("Element moved successfully")
sc.write_outline("my-story.stbx")  # Save changes
```

</div>

---

## Search Operations

### SearchForText

Searches for text across all elements.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchForText(string searchText)
```

```python
def search_for_text(text):
    ...
```

</div>

**Returns:** List of dictionaries containing matching elements and properties.

---

### SearchForReferences

Finds elements that reference a target element.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchForReferences(Guid targetUuid)
```

```python
def search_for_references(element):
    ...
```

</div>

---

### SearchInSubtree

Searches within a subtree of the outline.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchInSubtree(
    Guid rootNodeGuid,
    string searchText)
```

```python
def search_in_subtree(element, text):
    ...
```

</div>

---

### RemoveReferences

Removes all references to a target element from other elements in the outline.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<int> RemoveReferences(Guid targetUuid)
```

```python
def remove_references(element):
    ...  # returns the number of references removed
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `targetUuid` | Guid | GUID of the element whose references should be removed |

**Returns:** `OperationResult<int>` - Number of references removed

---

## Relationship Operations

### AddCastMember

Adds a character to a scene's cast.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> AddCastMember(Guid scene, Guid character)
```

```python
def add_cast_member(scene, character):
    ...
```

</div>

---

### AddRelationship

Creates a relationship between two characters.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> AddRelationship(
    Guid source,
    Guid recipient,
    string desc,
    bool mirror = false)
```

```python
def add_relationship(a, b, desc, mirror=False):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `source` | Guid | First character's GUID |
| `recipient` | Guid | Second character's GUID |
| `desc` | string | Description of relationship |
| `mirror` | bool | If true, creates reciprocal relationship |

---

## Trash Operations

### RestoreFromTrash

Restores a deleted element from the trash.

<div class="code-tabs" markdown="1">

```csharp
public Task<OperationResult<bool>> RestoreFromTrash(Guid elementToRestore)
```

```python
def restore_from_trash(element):
    ...
```

</div>

---

### EmptyTrash

Permanently deletes all trashed elements.

<div class="code-tabs" markdown="1">

```csharp
public Task<OperationResult<bool>> EmptyTrash()
```

```python
def empty_trash():
    ...
```

</div>

---

## Resource Data Methods

These methods provide access to StoryCAD's built-in reference data.

### GetExamples

Gets example values for a property.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<IEnumerable<string>> GetExamples(string propertyName)
```

```python
def get_examples(property_name):
    ...
```

</div>

---

### GetConflictCategories

Gets available conflict type categories.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<IEnumerable<string>> GetConflictCategories()
```

```python
def get_conflict_categories():
    ...
```

</div>

---

### GetKeyQuestionElements

Gets key question prompts for story development.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<IEnumerable<string>> GetKeyQuestionElements()
```

```python
def get_key_question_elements():
    ...
```

</div>

---

## Internal / Advanced Methods

These methods are used internally by StoryCAD and the Collaborator plugin. They are available in the public API but most consumers should use the standard methods above.

### SetCurrentModel

Sets the active StoryModel. Used by the Collaborator plugin to synchronize the API with StoryCAD's active outline.

<div class="code-tabs" markdown="1">

```csharp
public void SetCurrentModel(StoryModel model)
```

```python
# (no direct Python equivalent — this is a .NET Collaborator-plugin
# synchronization hook used internally by StoryCAD)
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `model` | StoryModel | The active StoryModel from ShellViewModel |

> **Note:** This is one of the few API methods that does not return `OperationResult<T>`.

---

### DeleteStoryElement

Moves an element to the trash. This is an older variant that accepts a string GUID. Prefer `DeleteElement(Guid)` for new code.

<div class="code-tabs" markdown="1">

```csharp
public OperationResult<bool> DeleteStoryElement(string uuid)
```

```python
# Older string-GUID variant; prefer delete_element(element) for new code.
def delete_element(element):
    ...
```

</div>

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `uuid` | string | The element's GUID as a string |

**Returns:** `OperationResult<bool>` - true on success
