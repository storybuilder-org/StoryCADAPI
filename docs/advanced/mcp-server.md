# MCP Server (StoryCADMcp)

StoryCADMcp is a [Model Context Protocol](https://modelcontextprotocol.io/) server that exposes the StoryCAD API as MCP tools. This allows any MCP-compatible client — such as Claude Desktop, Claude Code, or other LLM-powered tools — to read, write, and manage story outlines through natural language.

## Overview

The server wraps `StoryCADApi` and exposes 18 tools across four categories:

| Category | Tools | Purpose |
|----------|-------|---------|
| Outline | 3 | Open, save, and close outline files |
| Read | 5 | List, search, and inspect elements |
| Write | 7 | Add, update, delete, move, and link elements |
| Resource | 6 | Query writing reference data (plots, beats, conflicts, etc.) |

The server uses **stdio transport**, communicating with the client over standard input/output.

## Configuration

### Claude Desktop

Add this to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "storycad": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/StoryCADAPI/StoryCADMcp"]
    }
  }
}
```

### Claude Code

Add to your `.mcp.json` or configure via the CLI:

```json
{
  "mcpServers": {
    "storycad": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/StoryCADAPI/StoryCADMcp"]
    }
  }
}
```

Replace `/path/to/StoryCADAPI/StoryCADMcp` with the actual path to the StoryCADMcp project directory.

---

## Tool Reference

### Outline Tools

These tools manage the outline lifecycle. An outline must be opened before any read or write tools can be used.

#### `open_outline`

Opens a StoryCAD outline file (.stbx) and loads it into memory.

| Parameter | Type | Description |
|-----------|------|-------------|
| `path` | string | Absolute path to the .stbx outline file |

**Returns:** Confirmation with element count.

---

#### `save_outline`

Saves the currently open outline to disk.

| Parameter | Type | Description |
|-----------|------|-------------|
| `path` | string | Optional. Path to save to. Omit to save to the original location. |

**Returns:** Confirmation with save path.

---

#### `close_outline`

Closes the currently open outline, clearing it from memory. Call `save_outline` first if you have unsaved changes.

**Returns:** Confirmation.

---

### Read Tools

These tools inspect the open outline without modifying it.

#### `list_elements`

Lists all story elements in the open outline. Optionally filter by type.

| Parameter | Type | Description |
|-----------|------|-------------|
| `type` | string | Optional. Filter by element type: `Problem`, `Character`, `Setting`, `Scene`, `Folder`, `Section`, `Web`, `Notes`, `StoryOverview`, `StoryWorld` |

**Returns:** JSON array of `{guid, name, type}` objects.

---

#### `get_element`

Gets full detail for a single story element by its GUID, including all properties and fields.

| Parameter | Type | Description |
|-----------|------|-------------|
| `guid` | string | The GUID of the element to retrieve |

**Returns:** Serialized element with all fields.

---

#### `search_text`

Searches all text fields in the open outline for the given text.

| Parameter | Type | Description |
|-----------|------|-------------|
| `text` | string | The text to search for across all element fields |

**Returns:** Matching elements and the fields that contain the text.

---

#### `get_structure`

Returns the explorer tree hierarchy of the open outline, showing parent-child relationships between all elements.

**Returns:** Nested JSON tree of `{guid, name, type, children}` objects.

---

#### `get_problem_structure`

Returns the beat structure for a Problem element, showing its title, description, and all beats with their linked elements.

| Parameter | Type | Description |
|-----------|------|-------------|
| `guid` | string | The GUID of the Problem element |

**Returns:** JSON with `{title, description, beats[]}`.

---

### Write Tools

These tools modify the open outline. Call `save_outline` after making changes to persist them.

#### `add_element`

Adds a new story element as a child of the specified parent.

| Parameter | Type | Description |
|-----------|------|-------------|
| `type` | string | Element type: `Problem`, `Character`, `Setting`, `Scene`, `Folder`, `Section`, `Web`, `Notes` |
| `parentGuid` | string | GUID of the parent element |
| `name` | string | Name for the new element |

**Returns:** JSON with `{guid, name, type}` of the created element.

---

#### `update_property`

Updates a single property on a story element.

| Parameter | Type | Description |
|-----------|------|-------------|
| `guid` | string | GUID of the element to update |
| `property` | string | Property name (e.g. `Name`, `Description`, `Role`, `Archetype`) |
| `value` | string | New value for the property |

**Returns:** Confirmation with `{guid, property, updated}`.

---

#### `delete_element`

Moves a story element to the trash.

| Parameter | Type | Description |
|-----------|------|-------------|
| `guid` | string | GUID of the element to delete |

**Returns:** Confirmation with `{guid, deleted}`.

---

#### `move_element`

Moves a story element to a new parent in the outline tree.

| Parameter | Type | Description |
|-----------|------|-------------|
| `guid` | string | GUID of the element to move |
| `newParentGuid` | string | GUID of the new parent element |

**Returns:** Confirmation with `{guid, newParent, moved}`.

---

#### `link_cast`

Adds a character to a scene's cast list.

| Parameter | Type | Description |
|-----------|------|-------------|
| `sceneGuid` | string | GUID of the scene |
| `characterGuid` | string | GUID of the character |

**Returns:** Confirmation with `{scene, character, linked}`.

---

#### `add_relationship`

Creates a bidirectional relationship between two characters.

| Parameter | Type | Description |
|-----------|------|-------------|
| `sourceGuid` | string | GUID of the first character |
| `targetGuid` | string | GUID of the second character |
| `description` | string | Description of the relationship (e.g. "sister", "rival") |

**Returns:** Confirmation with `{source, target, description, linked}`.

---

#### `apply_beat_sheet`

Applies a named beat sheet template to a Problem element, creating its narrative structure.

| Parameter | Type | Description |
|-----------|------|-------------|
| `problemGuid` | string | GUID of the Problem element |
| `beatSheetName` | string | Name of the beat sheet (use `get_beat_sheets` to see available names) |

**Returns:** Confirmation with `{problem, beatSheet, applied}`.

---

### Resource Tools

These tools provide access to StoryCAD's built-in writing reference data. They do not require an open outline.

#### `get_key_questions`

Gets key questions for developing story elements.

| Parameter | Type | Description |
|-----------|------|-------------|
| `elementType` | string | Optional. Element type (e.g. `Character`, `Problem`). Omit to list available types. |

**Returns:** List of types, or `{topic, question}` pairs for the specified type.

---

#### `get_master_plots`

Gets master plot patterns for story structure.

| Parameter | Type | Description |
|-----------|------|-------------|
| `plotName` | string | Optional. Plot name. Omit to list all available master plots. |

**Returns:** List of plot names, or `{name, notes, scenes[]}` for the specified plot.

---

#### `get_beat_sheets`

Gets beat sheet templates for structuring narrative.

| Parameter | Type | Description |
|-----------|------|-------------|
| `sheetName` | string | Optional. Beat sheet name. Omit to list all available sheets. |

**Returns:** List of sheet names, or `{name, description, beats[]}` for the specified sheet.

---

#### `get_conflict_categories`

Gets conflict types for story problems. Supports three levels of detail.

| Parameter | Type | Description |
|-----------|------|-------------|
| `category` | string | Optional. Conflict category (e.g. "Person vs. Person") |
| `subcategory` | string | Optional. Subcategory within the category |

**Returns:** Categories, subcategories, or conflict examples depending on arguments provided.

---

#### `get_stock_scenes`

Gets stock scene ideas for common story situations.

| Parameter | Type | Description |
|-----------|------|-------------|
| `category` | string | Optional. Category name. Omit to list all categories. |

**Returns:** List of categories, or scene ideas for the specified category.

---

#### `get_examples`

Gets valid example values for a story element property.

| Parameter | Type | Description |
|-----------|------|-------------|
| `propertyName` | string | Property name (e.g. `Role`, `Archetype`, `Tone`, `ScenePurpose`) |

**Returns:** List of valid values for the property.

---

## Architecture

StoryCADMcp bridges two dependency injection containers:

1. **StoryCADLib DI** — initialized via `BootStrapper.Initialise(headless: true)`, which populates `Ioc.Default` with all StoryCADLib services
2. **MCP Host DI** — the .NET Generic Host container, which gets the `StoryCADApi` singleton registered into it

```
Program.cs
├── BootStrapper.Initialise(headless: true)   ← StoryCADLib DI
├── Ioc.Default.GetRequiredService<StoryCADApi>()
└── Host.CreateApplicationBuilder()
    ├── services.AddSingleton(api)             ← Bridge into MCP DI
    └── services.AddMcpServer()
        ├── WithStdioServerTransport()
        └── WithToolsFromAssembly()            ← Discovers [McpServerToolType] classes
```

Tool classes are static and receive `StoryCADApi` via MCP's constructor injection into tool method parameters.

## Dependencies

```xml
<PackageReference Include="ModelContextProtocol" Version="1.1.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.0" />
```

StoryCADLib is referenced as a `ProjectReference` (not a NuGet package), so it always tracks the current source.

## See Also

- [Semantic Kernel Integration](semantic-kernel.md) — alternative integration via SK plugins
- [StoryCADApi Reference](../api/storycad-api.md) — underlying API documentation
