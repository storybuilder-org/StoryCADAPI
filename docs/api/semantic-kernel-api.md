# SemanticKernelApi Class

The main entry point for StoryCADLib API operations.

```csharp
namespace StoryCADLib.Services.API

public class SemanticKernelApi : IStoryCADAPI
```

## Overview

`SemanticKernelApi` provides methods for creating, reading, updating, and deleting story elements. All methods are decorated with `[KernelFunction]` for Semantic Kernel integration, but can also be called directly.

**Key Concepts:**
- All methods return `OperationResult<T>` - check `IsSuccess` before using `Payload`
- The API maintains a `CurrentModel` property holding the active story
- Set `CurrentModel` via `CreateEmptyOutline()` or `OpenOutline()` before other operations

## Getting an Instance

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize (once at startup)
ServiceLocator.Initialize(headless: true);

// Get the API instance
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();
```

---

## Outline Operations

### CreateEmptyOutline

Creates a new empty story outline based on a template.

```csharp
public async Task<OperationResult<List<Guid>>> CreateEmptyOutline(
    string name,
    string author,
    string templateIndex)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `name` | string | Title of the story |
| `author` | string | Author name |
| `templateIndex` | string | Template index: "0" for blank, "1" for basic |

**Returns:** `OperationResult<List<Guid>>` - GUIDs of created elements

**Example:**
```csharp
var result = await api.CreateEmptyOutline("My Story", "Jane Doe", "0");
if (result.IsSuccess)
{
    Console.WriteLine($"Created {result.Payload.Count} elements");
}
```

---

### OpenOutline

Opens an existing story outline from disk.

```csharp
public async Task<OperationResult<bool>> OpenOutline(string path)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `path` | string | Full path to the .stbx file |

**Returns:** `OperationResult<bool>` - true on success

**Example:**
```csharp
var result = await api.OpenOutline("C:/Stories/my-story.stbx");
if (result.IsSuccess)
{
    var elements = api.GetAllElements();
}
```

---

### WriteOutline

Saves the current outline to disk.

```csharp
public async Task<OperationResult<string>> WriteOutline(string filePath)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `filePath` | string | Full path for the .stbx file |

**Returns:** `OperationResult<string>` - Success message

**Example:**
```csharp
var result = await api.WriteOutline("C:/Stories/my-story.stbx");
if (!result.IsSuccess)
{
    Console.WriteLine($"Save failed: {result.ErrorMessage}");
}
```

---

## Element Operations

### GetAllElements

Returns all elements in the current model.

```csharp
public OperationResult<ObservableCollection<StoryElement>> GetAllElements()
```

**Returns:** `OperationResult<ObservableCollection<StoryElement>>` - All story elements

---

### GetElementsByType

Gets all elements of a specific type.

```csharp
public OperationResult<List<StoryElement>> GetElementsByType(StoryItemType elementType)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementType` | StoryItemType | The type to filter by |

**Valid Types:** `Problem`, `Character`, `Setting`, `Scene`, `Folder`, `Section`, `Web`, `Notes`, `StoryWorld`

**Example:**
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

---

### GetStoryElement

Gets a specific element by its GUID.

```csharp
public OperationResult<StoryElement> GetStoryElement(Guid guid)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `guid` | Guid | The element's unique identifier |

**Returns:** `OperationResult<StoryElement>` - The requested element

---

### AddElement

Creates a new story element.

```csharp
public OperationResult<Guid> AddElement(
    StoryItemType typeToAdd,
    string parentGUID,
    string name,
    string description = "")
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `typeToAdd` | StoryItemType | Type of element to create |
| `parentGUID` | string | GUID of parent element (as string) |
| `name` | string | Name for the new element |
| `description` | string | Optional description |

**Returns:** `OperationResult<Guid>` - GUID of created element

**Example:**
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

---

### AddElement (with properties)

Creates a new story element with initial properties and an optional GUID override.

```csharp
public OperationResult<Guid> AddElement(
    StoryItemType typeToAdd,
    string parentGUID,
    string name,
    Dictionary<string, object> properties,
    string GUIDOverride = "")
```

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

---

### UpdateElementProperty

Updates a single property on an element.

```csharp
public OperationResult<StoryElement> UpdateElementProperty(
    Guid elementUuid,
    string propertyName,
    object value)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementUuid` | Guid | The element's GUID |
| `propertyName` | string | Name of the property to update |
| `value` | object | New value for the property |

**Returns:** `OperationResult<StoryElement>` - Updated element

**Example:**
```csharp
var result = api.UpdateElementProperty(
    characterGuid,
    "Role",
    "Protagonist");
```

---

### UpdateElementProperties

Updates multiple properties on an element.

```csharp
public OperationResult<bool> UpdateElementProperties(
    Guid elementGuid,
    Dictionary<string, object> properties)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementGuid` | Guid | The element's GUID |
| `properties` | Dictionary | Property names and values |

**Example:**
```csharp
var props = new Dictionary<string, object>
{
    { "Name", "Alex" },
    { "Role", "Protagonist" },
    { "Age", "32" }
};
api.UpdateElementProperties(characterGuid, props);
```

---

### DeleteElement

Moves an element to the trash.

```csharp
public Task<OperationResult<bool>> DeleteElement(Guid elementToDelete)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `elementToDelete` | Guid | The element's GUID |

**Returns:** `OperationResult<bool>` - true on success

---

### GetElement

Returns a single element with all its fields as a serialized object. Unlike `GetStoryElement` which returns a typed `StoryElement`, this returns the full serialized representation including all fields.

```csharp
public OperationResult<object> GetElement(Guid guid)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `guid` | Guid | The element's unique identifier |

**Returns:** `OperationResult<object>` - Serialized element with all fields

**Example:**
```csharp
var result = api.GetElement(characterGuid);
if (result.IsSuccess)
{
    Console.WriteLine(result.Payload); // Full serialized element
}
```

---

### UpdateStoryElement

Replaces an entire story element by deserializing a new element and updating it in the model. For updating individual properties, prefer `UpdateElementProperty` or `UpdateElementProperties`.

```csharp
public OperationResult<bool> UpdateStoryElement(object newElement, Guid guid)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `newElement` | object | Serialized element data (will be deserialized via `StoryElement.Deserialize`) |
| `guid` | Guid | The GUID of the element to replace |

**Returns:** `OperationResult<bool>` - true on success

---

### MoveElement

Moves an element to a new parent in the outline's ExplorerView tree. Validates against circular references, moving the root element, and moving the TrashCan.

```csharp
public OperationResult<bool> MoveElement(Guid elementGuid, Guid newParentGuid)
```

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
```csharp
// Move a scene under a different folder
var result = api.MoveElement(sceneGuid, newFolderGuid);
if (result.IsSuccess)
{
    Console.WriteLine("Element moved successfully");
    await api.WriteOutline("my-story.stbx"); // Save changes
}
```

---

## Search Operations

### SearchForText

Searches for text across all elements.

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchForText(string searchText)
```

**Returns:** List of dictionaries containing matching elements and properties.

---

### SearchForReferences

Finds elements that reference a target element.

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchForReferences(Guid targetUuid)
```

---

### SearchInSubtree

Searches within a subtree of the outline.

```csharp
public OperationResult<List<Dictionary<string, object>>> SearchInSubtree(
    Guid rootNodeGuid,
    string searchText)
```

---

### RemoveReferences

Removes all references to a target element from other elements in the outline.

```csharp
public OperationResult<int> RemoveReferences(Guid targetUuid)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `targetUuid` | Guid | GUID of the element whose references should be removed |

**Returns:** `OperationResult<int>` - Number of references removed

---

## Relationship Operations

### AddCastMember

Adds a character to a scene's cast.

```csharp
public OperationResult<bool> AddCastMember(Guid scene, Guid character)
```

---

### AddRelationship

Creates a relationship between two characters.

```csharp
public OperationResult<bool> AddRelationship(
    Guid source,
    Guid recipient,
    string desc,
    bool mirror = false)
```

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

```csharp
public Task<OperationResult<bool>> RestoreFromTrash(Guid elementToRestore)
```

---

### EmptyTrash

Permanently deletes all trashed elements.

```csharp
public Task<OperationResult<bool>> EmptyTrash()
```

---

## Resource Data Methods

These methods provide access to StoryCAD's built-in reference data.

### GetExamples

Gets example values for a property.

```csharp
public OperationResult<IEnumerable<string>> GetExamples(string propertyName)
```

---

### GetConflictCategories

Gets available conflict type categories.

```csharp
public OperationResult<IEnumerable<string>> GetConflictCategories()
```

---

### GetKeyQuestionElements

Gets key question prompts for story development.

```csharp
public OperationResult<IEnumerable<string>> GetKeyQuestionElements()
```

---

## Internal / Advanced Methods

These methods are used internally by StoryCAD and the Collaborator plugin. They are available in the public API but most consumers should use the standard methods above.

### SetCurrentModel

Sets the active StoryModel. Used by the Collaborator plugin to synchronize the API with StoryCAD's active outline.

```csharp
public void SetCurrentModel(StoryModel model)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `model` | StoryModel | The active StoryModel from ShellViewModel |

> **Note:** This is one of the few API methods that does not return `OperationResult<T>`.

---

### DeleteStoryElement

Moves an element to the trash. This is an older variant that accepts a string GUID. Prefer `DeleteElement(Guid)` for new code.

```csharp
public OperationResult<bool> DeleteStoryElement(string uuid)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `uuid` | string | The element's GUID as a string |

**Returns:** `OperationResult<bool>` - true on success
