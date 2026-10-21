---
layout: default
title: "MCP Server"
parent: "Advanced"
nav_order: 2
---

# MCP Server (StoryCADMcp)

StoryCADMcp is a [Model Context Protocol](https://modelcontextprotocol.io/) server that exposes the StoryCAD API as MCP tools. This allows any MCP-compatible client (such as Claude Desktop, Claude Code, or other LLM-powered tools) to read, write, and manage story outlines through natural language.

## Overview

The server wraps `StoryCADApi` and exposes 42 tools across six categories:

| Category | Tools | Purpose |
|----------|-------|---------|
| Outline | 4 | Create, open, save, and close outline files |
| Read | 5 | List, search, and inspect elements |
| Write | 10 | Add, update, delete, move, link, and edit list-property entries |
| Beat | 8 | Create, edit, assign, move, save, and load the beats of a Problem |
| Resource | 6 | Query writing reference data (plots, beats, conflicts, etc.) |
| Collaborator | 9 | Run Collaborator workflows and accept or skip their proposals |

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

Closes the currently open outline, clearing it from memory, and ends the Collaborator session. Call `save_outline` first if you have unsaved changes.

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

#### `add_collection_entry`

Adds an entry to a list-typed property on a story element (for example, adding a `PhysicalWorld` or `Culture` to a `StoryWorld`). The entry is supplied as a JSON object string and is converted to the collection's element type by the API.

| Parameter | Type | Description |
|-----------|------|-------------|
| `elementGuid` | string | GUID of the element that owns the collection |
| `propertyName` | string | Name of the list-typed property (e.g. `PhysicalWorlds`, `Cultures`, `Species`, `Governments`, `Religions`) |
| `entryJson` | string | JSON object representing the entry to add |

**Returns:** JSON with `{elementGuid, propertyName, index}` where `index` is the zero-based position of the new entry.

---

#### `update_collection_entry`

Replaces an existing entry at the given index in a list-typed property on a story element.

| Parameter | Type | Description |
|-----------|------|-------------|
| `elementGuid` | string | GUID of the element that owns the collection |
| `propertyName` | string | Name of the list-typed property |
| `index` | int | Zero-based index of the entry to replace |
| `entryJson` | string | JSON object representing the replacement entry |

**Returns:** Confirmation with `{elementGuid, propertyName, index, updated}`.

---

#### `remove_collection_entry`

Removes the entry at the given index from a list-typed property on a story element.

| Parameter | Type | Description |
|-----------|------|-------------|
| `elementGuid` | string | GUID of the element that owns the collection |
| `propertyName` | string | Name of the list-typed property |
| `index` | int | Zero-based index of the entry to remove |

**Returns:** Confirmation with `{elementGuid, propertyName, index, removed}`.

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

### Collaborator Tools

These tools run the same Collaborator workflows as the StoryCAD Collaborator window, through the same code. A run sends story elements to the Collaborator Worker, which returns proposals: a new value for one property of one element. Nothing is written to the outline until you call `accept` or `accept_all`.

**Requirements.** Collaborator is a paid feature. The server uses the Collaborator sign-in of the StoryCAD app installed on the same machine, so sign in to Collaborator in the app once before you use these tools. At start the server copies four sign-in keys (`StoreUserGuid`, `StoreActivationJwt`, `StoreActivationJwtExpiry`, `StoreActivationPath`) from the app's `Preferences.json`, and it reads them again before each run. It never writes the app's file, and no tool result contains the token. When the token has expired, a run returns "Open StoryCAD once to refresh the Collaborator sign-in." Each run is charged to your Collaborator credit, as in the app.

| Platform | Preferences file the server reads |
|----------|-----------------------------------|
| Windows, store app | `%LOCALAPPDATA%\Packages\34432StoryBuilder.StoryBuilder_*\RoamingState\StoryCAD\Preferences.json` |
| Windows, unpackaged build | `%APPDATA%\StoryCAD\Preferences.json` |
| macOS | `~/Library/Application Support/StoryCAD/Preferences.json` |

**Worker.** The server uses the Worker that the app uses. To use another Worker, set `COLLAB_PROXY_URL` in the `env` block of the MCP entry. The Worker chooses the model. The server tells the Worker that a call came from the MCP server, so the Worker can count that use; this does not change the price.

**One session at a time.** A new `run_workflow` ends the session before it. `close_outline` ends it too.

**Backup.** Before the first write to an outline (`accept`, `accept_all`, or `save_outline` during a session), the server makes one backup of the outline file in the StoryCAD backup folder. An outline that has never been saved has no file to back up.

**Run time and cost.** `run_workflow` and `try_again` return the run time in seconds and the cost in dollars, from the Worker's cost report. `chat` returns the time only.

A typical session: `open_outline`, `list_workflows`, `run_workflow`, `get_proposals`, then `accept` or `skip` for each key, then `save_outline`.

#### `list_workflows`

Lists the Collaborator workflows.

**Returns:** JSON array of `{label, title, description, elementType, requiredInputs[], optionalInputs[]}`. Each input is `{name, type}`.

---

#### `run_workflow`

Runs a workflow on one element of the open outline and keeps its proposals. Takes a minute or more and costs credit. The Character Interview workflow is not available through this server; use the Collaborator window.

| Parameter | Type | Description |
|-----------|------|-------------|
| `workflow` | string | Workflow label from `list_workflows` |
| `element` | string | GUID of the element the run is about (from `list_elements`) |
| `inputs` | object | Optional. Input name to element GUID, for inputs the run cannot find on its own. |

If an input cannot be found, the tool returns an error that names it. Find the GUID with `list_elements` and pass it in `inputs`.

**Returns:** JSON with `{workflow, proposals[], kept[], seconds, cost}`. `kept` lists the properties the Worker left as they are.

---

#### `get_proposals`

Lists each proposal of the session.

**Returns:** JSON array of `{key, element, property, state, current, proposed, explanation, status}`. `state` is `New` (the property is empty), `Refresh` (Collaborator wrote it earlier in this session), or `Has your text` (the proposal replaces text you wrote). `status` is `Open`, `Accepted`, or `Skipped`.

---

#### `accept`

Accepts one proposal and writes it to the outline.

| Parameter | Type | Description |
|-----------|------|-------------|
| `key` | string | Proposal key from `get_proposals` |
| `overwrite_your_text` | bool | Optional, default false. A proposal in state `Has your text` is written only when this is true. |

With `overwrite_your_text` false, a `Has your text` proposal returns an error and writes nothing. An unknown key returns an error with the open keys.

**Returns:** The proposal after the write.

---

#### `skip`

Skips one proposal. The outline keeps its text.

| Parameter | Type | Description |
|-----------|------|-------------|
| `key` | string | Proposal key from `get_proposals` |

**Returns:** The proposal after the skip.

---

#### `accept_all`

Accepts every open proposal.

| Parameter | Type | Description |
|-----------|------|-------------|
| `overwrite_your_text` | bool | Optional, default false. With false, `Has your text` proposals stay open. |

**Returns:** JSON with `{applied[], open[]}`: the keys the call wrote and the keys that stay open.

---

#### `try_again`

Runs the workflow again. The open and skipped proposals go to the Worker as what to depart from. Takes a minute or more and costs credit.

**Returns:** The same JSON as `run_workflow`.

---

#### `chat`

Sends one chat message about the proposals. The reply can change proposals; it never writes the outline. Costs credit.

| Parameter | Type | Description |
|-----------|------|-------------|
| `text` | string | The message |

**Returns:** JSON with `{reply, changed[], seconds}`. `changed` lists the keys whose proposed text or status the reply changed.

---

#### `settings`

Shows or changes the Collaborator settings that the next run sends. Omit an argument to leave it as it is.

| Parameter | Type | Description |
|-----------|------|-------------|
| `terseness` | string | Optional. `Concise`, `Balanced`, or `Detailed` |
| `genre_preferences` | string | Optional. Preferred genres, comma-separated |
| `story_form_likes` | string | Optional. Story forms you like, comma-separated |
| `story_form_dislikes` | string | Optional. Story forms you want to avoid, comma-separated |

**Returns:** JSON with the four current values.

---

## Architecture

StoryCADMcp bridges two dependency injection containers:

1. **StoryCADLib DI**: initialized via `BootStrapper.Initialise(headless: true)`, which populates `Ioc.Default` with all StoryCADLib services
2. **MCP Host DI**: the .NET Generic Host container, which gets the `StoryCADApi` singleton registered into it

```
Program.cs
├── STORYCAD_ROOT_DIR = %LOCALAPPDATA%/StoryCADMcp (honored by Debug builds of StoryCADLib)
├── BootStrapper.Initialise(headless: true)   ← StoryCADLib DI
├── KernelFactory.ClientName = "mcp"          ← the Worker journal counts MCP use
├── CollaboratorSignIn.EnsureAsync()          ← copies the app's Collaborator sign-in
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

StoryCADLib is referenced as a NuGet package. `UseStoryCADLibNuGet` in `Directory.Build.props` is true, and `StoryCADLibVersion` in the same file pins the version. The Collaborator tools need a package that carries `CollaboratorLib.dll`, which starts with 4.4.0.

## See Also

- [Semantic Kernel Integration](semantic-kernel.md): alternative integration via SK plugins
- [StoryCADApi Reference](../api/storycad-api.md): underlying API documentation
