---
layout: default
title: Models.Scrivener
parent: API Reference
nav_order: 5
---

# Models.Scrivener

Auto-generated reference for the `StoryCADLib.Models.Scrivener` namespace (2 types).

{: .fs-6 .fw-300 }

## BinderItem

*Class* — `StoryCADLib.Models.Scrivener.BinderItem`

A simple data transfer object (DTO) that contains raw data about a BinderItem

```csharp
public class BinderItem
```

### Constructors

#### BinderItem(string, BinderItemType, string)

```csharp
public BinderItem(string uuid, BinderItemType type, string header)
```

**Parameters**

- `uuid` (`string`)
- `type` (`BinderItemType`)
- `header` (`string`)

#### BinderItem(string, BinderItemType, string, BinderItem)

```csharp
public BinderItem(string uuid, BinderItemType type, string header, BinderItem parent)
```

**Parameters**

- `uuid` (`string`)
- `type` (`BinderItemType`)
- `header` (`string`)
- `parent` (`BinderItem`)

#### BinderItem(string, BinderItemType, string, BinderItem, string, string, string)

```csharp
public BinderItem(string uuid, BinderItemType type, string header, BinderItem parent, string created, string modified, string stbUuid)
```

**Parameters**

- `uuid` (`string`)
- `type` (`BinderItemType`)
- `header` (`string`)
- `parent` (`BinderItem`)
- `created` (`string`)
- `modified` (`string`)
- `stbUuid` (`string`)

### Properties

#### Parent

```csharp
public BinderItem Parent { get; set; }
```

**Type** `BinderItem`

#### Children

```csharp
public List<BinderItem> Children { get; }
```

**Type** `List<BinderItem>`

#### Uuid

```csharp
public string Uuid { get; set; }
```

**Type** `string`

#### Created

```csharp
public string Created { get; set; }
```

**Type** `string`

#### Modified

```csharp
public string Modified { get; set; }
```

**Type** `string`

#### Title

The displayed node name (Header property)

```csharp
public string Title { get; set; }
```

**Type** `string`

#### Type

```csharp
public BinderItemType Type { get; set; }
```

**Type** `BinderItemType`

#### StbUuid

```csharp
public string StbUuid { get; set; }
```

**Type** `string`

#### Node

```csharp
public IXmlNode Node { get; set; }
```

**Type** `IXmlNode`

### Methods

#### GetEnumerator()

This method allows a dept-first search (DFS) or 'pre-order traversal' of
a of a BinderItem tree or subtree with a simple C# foreach.
In a DFS, you visit the root first and then search deeper into the tree
visiting each node and then the node’s children.
To use the enumerator, you code a foreach loop anywhere in your program:
foreach (TreeNode node in root)
{
//perform action on node in DFS order here
}
ref: http://www.timlabonne.com/2013/07/performing-a-dfs-over-a-rooted-tree-with-a-c-foreach-loop/

```csharp
public IEnumerator<BinderItem> GetEnumerator()
```

**Returns** `IEnumerator<BinderItem>` — BinderItem for current node, then for each child in a loop

#### ToString()

```csharp
public override string ToString()
```

**Returns** `string`

---

## BinderItemType

*Enum* — `StoryCADLib.Models.Scrivener.BinderItemType`

```csharp
public enum BinderItemType
```

### Values

| Value | Description |
|-------|-------------|
| `Text` |  |
| `Folder` |  |
| `DraftFolder` |  |
| `ResearchFolder` |  |
| `TrashFolder` |  |
| `Pdf` |  |
| `WebArchive` |  |
| `Root` |  |
| `Unknown` |  |
