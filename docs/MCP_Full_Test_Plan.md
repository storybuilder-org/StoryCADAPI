# StoryCAD MCP Full Test Plan
**Time**: ~90–120 minutes
**Purpose**: Exercise the full StoryCAD MCP tool surface end-to-end. Verifies the StoryCADAPI through the `mcp__storycad__*` tools the way an external client would call them.

This plan starts with the same basic ops covered in [MCP_Smoke_Test.md](./MCP_Smoke_Test.md) (Section 1), then expands to cover element types, property edits, collections, relationships, beats, beat sheets, structure reads, search, reference data, persistence round-trip, and error cases.

## Test Conventions
- Each test names the MCP tool(s) used and the key arguments.
- "Expected" describes a successful response shape, not exact JSON.
- Tests are sequential — later tests depend on state created by earlier tests. Stop on first failure.
- Use `/tmp/mcp-full/` (or platform equivalent) as the scratch directory.
- Record GUIDs returned by `add_element` calls — later tests reference them.

## Prerequisites
- `storycad` MCP server registered and reachable (`claude mcp list` shows ✓).
- A writable scratch directory.
- An existing `.stbx` to seed from. `open_outline` does not create new files — before running, copy a sample (e.g., `StoryCAD/StoryCADLib/Assets/Install/samples/Hamlet.stbx`) into the scratch dir as `FullTest.stbx`.
- No outline currently open in the host process.

## What This Plan Does NOT Cover

The MCP surface is API-only. The following are **out of scope** and require the manual UI test plans in `StoryCADTests/ManualTests/`:

- **UI interactions**: drag-and-drop, context menus, tab switching, navigation tree expand/collapse, keyboard shortcuts, tooltips.
- **Window/dialog behavior**: file pickers, confirmation dialogs, Save-changes prompts, window resize, multi-monitor.
- **Reports rendering**: PDF/Print/Scrivener export (no MCP equivalent — `get_structure` / `get_problem_structure` return data, not rendered output).
- **Tools dialogs**: Inner/Outer Traits picker, Conflict Builder dialog, Narrative Editor, Dramatic Situations dialog (UI-only — the underlying reference data is reachable via `get_*` tools).
- **Preferences and services**: AutoSave, BackupService, theme, telemetry, license validation.
- **Cross-platform UI differences**: Windows vs macOS look-and-feel, native dialog shapes, Cmd vs Ctrl shortcuts.
- **Multi-instance scenarios**: concurrent access from multiple clients, file locking, single-instancing.
- **Restore-from-trash**: not currently exposed via MCP (`delete_element` is tested, restore is not).
- **Copy Elements** between outlines: not currently exposed via MCP.
- **Generate Reports** menu actions: PDF/Scrivener export. See `Reports_Test_Plan.md`.
- **Story Narrator view ordering**: covered by UI plan, not exposed via MCP.

---

## Section 1: Outline Lifecycle (Smoke)

This section duplicates the smoke test so the full plan stands alone. If the smoke test has already passed in this session, skim Section 1 and verify.

### FT-001: Server reachable, tools enumerated
**Priority:** Critical
**Tool:** (host listing)

**Steps:**
1. Confirm `storycad` appears in the MCP tool list. Tools should include all of: `open_outline`, `close_outline`, `save_outline`, `add_element`, `delete_element`, `move_element`, `get_element`, `list_elements`, `update_property`, `search_text`, `add_collection_entry`, `update_collection_entry`, `remove_collection_entry`, `add_relationship`, `link_cast`, `get_structure`, `get_problem_structure`, `create_beat`, `update_beat`, `delete_beat`, `move_beat`, `assign_beat`, `clear_beat`, `apply_beat_sheet`, `load_beat_sheet`, `save_beat_sheet`, `get_beat_sheets`, `get_master_plots`, `get_conflict_categories`, `get_key_questions`, `get_stock_scenes`, `get_examples`.
   **Expected:** All listed tools are present.

**Pass/Fail:** ______

---

### FT-002: Open new outline (blank template)
**Priority:** Critical
**Tool:** `open_outline`

**Steps:**
1. Call `open_outline` with `path = /tmp/mcp-full/FullTest.stbx`, template index 0 (blank).
   **Expected:** Success; outline is now active.
2. Call `list_elements`.
   **Expected:** Tree contains the default Story Overview / Trash root structure for a blank outline.

**Pass/Fail:** ______

---

### FT-003: Save outline
**Priority:** Critical
**Tool:** `save_outline`

**Steps:**
1. Call `save_outline`.
   **Expected:** Success; `/tmp/mcp-full/FullTest.stbx` exists on disk and is non-empty.

**Pass/Fail:** ______

---

### FT-004: Close outline
**Priority:** Critical
**Tool:** `close_outline`

**Steps:**
1. Call `close_outline`.
   **Expected:** Success.

**Pass/Fail:** ______

---

### FT-005: Reopen and verify
**Priority:** Critical
**Tool:** `open_outline`, `list_elements`

**Steps:**
1. Call `open_outline` with the saved path.
   **Expected:** Success.
2. `list_elements`.
   **Expected:** Same root structure as FT-002.

**Pass/Fail:** ______

---

## Section 2: Element Creation — All Types

Record the GUID returned by each `add_element` call. They are used in Sections 3–7.

### FT-010: Add Character
**Priority:** Critical
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Character`, `name = "Alice"`, parent = Story Overview GUID.
   **Expected:** Returns a new GUID. Record as `CHAR_ALICE`.

**Pass/Fail:** ______

---

### FT-011: Add second Character
**Priority:** High
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Character`, `name = "Bob"`, parent = Story Overview GUID.
   **Expected:** Returns a new GUID. Record as `CHAR_BOB`.

**Pass/Fail:** ______

---

### FT-012: Add Problem
**Priority:** Critical
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Problem`, `name = "Main Conflict"`, parent = Story Overview GUID.
   **Expected:** Returns a new GUID. Record as `PROB_MAIN`.

**Pass/Fail:** ______

---

### FT-013: Add sub-Problem
**Priority:** High
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Problem`, `name = "Sub Problem A"`, parent = `PROB_MAIN`.
   **Expected:** Returns a GUID. Record as `PROB_SUB`.

**Pass/Fail:** ______

---

### FT-014: Add Scene
**Priority:** Critical
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Scene`, `name = "Opening Scene"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `SCENE_OPEN`.

**Pass/Fail:** ______

---

### FT-015: Add second Scene
**Priority:** High
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Scene`, `name = "Midpoint Scene"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `SCENE_MID`.

**Pass/Fail:** ______

---

### FT-016: Add Setting
**Priority:** High
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Setting`, `name = "Coffee Shop"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `SET_COFFEE`.

**Pass/Fail:** ______

---

### FT-017: Add Folder
**Priority:** Medium
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Folder`, `name = "Backstory"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `FOLDER_BACK`.

**Pass/Fail:** ______

---

### FT-018: Add Section
**Priority:** Medium
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Section`, `name = "Act One"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `SEC_ACT1`.

**Pass/Fail:** ______

---

### FT-019: Add Web element
**Priority:** Medium
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Web`, `name = "Research Link"`, parent = `FOLDER_BACK`.
   **Expected:** Returns a GUID. Record as `WEB_LINK`.

**Pass/Fail:** ______

---

### FT-020: Add Notes element
**Priority:** Medium
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Notes`, `name = "Research Notes"`, parent = `FOLDER_BACK`.
   **Expected:** Returns a GUID. Record as `NOTES_RES`.

**Pass/Fail:** ______

---

### FT-021: Add StoryWorld
**Priority:** Critical
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = StoryWorld`, `name = "Test World"`, parent = Story Overview GUID.
   **Expected:** Returns a GUID. Record as `WORLD_MAIN`. StoryWorld is the canonical owner of the indexed list properties (PhysicalWorlds, Cultures, Species, Governments, Religions) tested in Section 5.

**Pass/Fail:** ______

---

## Section 3: Element Listing & Inspection

### FT-030: list_elements returns all created
**Priority:** Critical
**Tool:** `list_elements`

**Steps:**
1. Call `list_elements`.
   **Expected:** Response contains all elements created in Section 2 (Alice, Bob, Main Conflict, Sub Problem A, Opening Scene, Midpoint Scene, Coffee Shop, Backstory, Act One, Research Link, Research Notes) plus the Story Overview and Trash roots.

**Pass/Fail:** ______

---

### FT-031: get_element on Character
**Priority:** Critical
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `CHAR_ALICE`.
   **Expected:** Response shows `Name = "Alice"` and `ElementType = Character` (or equivalent type indicator).

**Pass/Fail:** ______

---

### FT-032: get_element on Problem
**Priority:** Critical
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `PROB_MAIN`.
   **Expected:** Response shows `Name = "Main Conflict"` and Problem-specific fields are present (e.g., StoryQuestion, ConflictType).

**Pass/Fail:** ______

---

### FT-033: get_element on Scene
**Priority:** High
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `SCENE_OPEN`.
   **Expected:** Response shows `Name = "Opening Scene"` and Scene-specific fields (ViewpointCharacter, Setting, etc.).

**Pass/Fail:** ______

---

### FT-034: get_element on Setting
**Priority:** Medium
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `SET_COFFEE`.
   **Expected:** Response shows `Name = "Coffee Shop"` and Setting-specific fields (Locale, Period, etc.).

**Pass/Fail:** ______

---

### FT-035: get_element on Web
**Priority:** Low
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `WEB_LINK`.
   **Expected:** Response shows `Name = "Research Link"` and a URL field is present (may be empty until set).

**Pass/Fail:** ______

---

### FT-036: get_element on StoryWorld
**Priority:** High
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `WORLD_MAIN`.
   **Expected:** Response shows `Name = "Test World"` and StoryWorld-specific list properties are present and empty: `PhysicalWorlds: []`, `Cultures: []`, `Species: []`, `Governments: []`, `Religions: []` (exact property names per the model).

**Pass/Fail:** ______

---

## Section 4: Property Updates

### FT-040: Set Character.Role
**Priority:** Critical
**Tool:** `update_property`, `get_element`

**Steps:**
1. `update_property` on `CHAR_ALICE`: set `Role = "Protagonist"`.
   **Expected:** Success.
2. `get_element` for `CHAR_ALICE`.
   **Expected:** `Role = "Protagonist"`.

**Pass/Fail:** ______

---

### FT-041: Set Character age/physical fields
**Priority:** High
**Tool:** `update_property`, `get_element`

**Steps:**
1. `update_property` on `CHAR_ALICE`: set `Age = "32"`, `Build = "Athletic"`.
   **Expected:** Success on each.
2. `get_element` for `CHAR_ALICE`.
   **Expected:** Both new values present.

**Pass/Fail:** ______

---

### FT-042: Set Problem.Description and ConflictType
**Priority:** Critical
**Tool:** `update_property`, `get_element`

**Steps:**
1. `update_property` on `PROB_MAIN`: set `Description = "Will Alice succeed?"`, `ConflictType = "Person vs. Person"`. (Note: `Description` is the model property; the UI labels it "Story Question" on Problem forms. `update_property` uses model names, not UI labels.)
   **Expected:** Success.
2. `get_element` for `PROB_MAIN`.
   **Expected:** `ElementDescription = "Will Alice succeed?"`, `ConflictType = "Person vs. Person"`.

**Pass/Fail:** ______

---

### FT-043: Set Scene.ViewpointCharacter (GUID-valued)
**Priority:** High
**Tool:** `update_property`, `get_element`

**Steps:**
1. `update_property` on `SCENE_OPEN`: set `ViewpointCharacter = CHAR_ALICE` (the Alice GUID).
   **Expected:** Success.
2. `get_element` for `SCENE_OPEN`.
   **Expected:** `ViewpointCharacter` resolves to Alice (by GUID).

**Pass/Fail:** ______

---

### FT-044: Set Scene.Setting
**Priority:** Medium
**Tool:** `update_property`

**Steps:**
1. `update_property` on `SCENE_OPEN`: set `Setting = SET_COFFEE`.
   **Expected:** Success.
2. `get_element` for `SCENE_OPEN`.
   **Expected:** `Setting` resolves to Coffee Shop.

**Pass/Fail:** ______

---

### FT-045: Update Description / summary text
**Priority:** Medium
**Tool:** `update_property`, `get_element`

**Steps:**
1. `update_property` on `CHAR_ALICE`: set `Description = "A determined detective with a hidden past."` (or the Description property used for Character per `reference_mcp_api_usage.md`).
   **Expected:** Success.
2. `get_element` for `CHAR_ALICE`.
   **Expected:** Description text round-trips.

**Pass/Fail:** ______

---

### FT-046: Rename via update_property Name
**Priority:** High
**Tool:** `update_property`, `list_elements`

**Steps:**
1. `update_property` on `SCENE_MID`: set `Name = "Midpoint Confrontation"`.
   **Expected:** Success.
2. `list_elements`.
   **Expected:** Element appears as "Midpoint Confrontation".

**Pass/Fail:** ______

---

## Section 5: Collection Entries (StoryWorld Indexed Lists)

This section targets the StoryWorld element type, which owns the indexed list properties the `*_collection_entry` tools were primarily designed for: `PhysicalWorlds`, `Cultures`, `Species`, `Governments`, `Religions`. Each entry is a structured object passed as JSON.

Note: Other element types also have list-typed properties (e.g., `Character.TraitList`, `Scene.CastMembers`). `CastMembers` is exercised through `link_cast` in Section 6. Generic list mutation on those non-StoryWorld lists is not the canonical use of these tools.

### FT-050: Add a PhysicalWorlds entry
**Priority:** Critical
**Tool:** `add_collection_entry`, `get_element`

**Steps:**
1. `add_collection_entry` on `WORLD_MAIN`: `propertyName = PhysicalWorlds`, `entryJson = {"Name":"Earth","Geography":"Varied"}` (use the actual fields the PhysicalWorld type expects).
   **Expected:** Success; returns `index = 0`.
2. `get_element` for `WORLD_MAIN`.
   **Expected:** `PhysicalWorlds` contains one entry with the supplied fields.

**Pass/Fail:** ______

---

### FT-051: Add entries to all three primary indexed lists
**Priority:** Critical
**Tool:** `add_collection_entry`, `get_element`

**Steps:**
1. `add_collection_entry` on `WORLD_MAIN`: `propertyName = PhysicalWorlds`, `entryJson = {"Name":"Mars","Geography":"Desert"}`.
   **Expected:** `index = 1`.
2. `add_collection_entry` on `WORLD_MAIN`: `propertyName = Cultures`, `entryJson = {"Name":"Northerners"}` (or the minimal Culture shape).
   **Expected:** `index = 0`.
3. `add_collection_entry` on `WORLD_MAIN`: `propertyName = Species`, `entryJson = {"Name":"Humans"}` (or the minimal Species shape).
   **Expected:** `index = 0`.
4. `get_element` for `WORLD_MAIN`.
   **Expected:** `PhysicalWorlds` has 2 entries; `Cultures` has 1 entry; `Species` has 1 entry.

**Pass/Fail:** ______

---

### FT-052: Update an indexed entry
**Priority:** High
**Tool:** `update_collection_entry`, `get_element`

**Steps:**
1. `update_collection_entry` on `WORLD_MAIN`: `propertyName = PhysicalWorlds`, `index = 1`, `entryJson = {"Name":"Mars","Geography":"Reddish desert with polar ice"}`.
   **Expected:** Success; `updated = true`.
2. `get_element` for `WORLD_MAIN`.
   **Expected:** `PhysicalWorlds[1].Geography` reflects the new value; `PhysicalWorlds[0]` unchanged.

**Pass/Fail:** ______

---

### FT-053: Remove an indexed entry
**Priority:** Critical
**Tool:** `remove_collection_entry`, `get_element`

**Steps:**
1. `remove_collection_entry` on `WORLD_MAIN`: `propertyName = PhysicalWorlds`, `index = 0` (remove "Earth").
   **Expected:** Success; `removed = true`.
2. `get_element` for `WORLD_MAIN`.
   **Expected:** `PhysicalWorlds` now has 1 entry, and that entry is the one previously at index 1 ("Mars" with the updated geography). Indices have shifted.

**Pass/Fail:** ______

---

### FT-054: Index out-of-range — graceful error
**Priority:** Medium
**Tool:** `update_collection_entry`, `remove_collection_entry`

**Steps:**
1. `remove_collection_entry` on `WORLD_MAIN`: `propertyName = PhysicalWorlds`, `index = 99`.
   **Expected:** Clean error indicating index is out of range. No crash.
2. `update_collection_entry` on `WORLD_MAIN`: `propertyName = Cultures`, `index = 99`, any `entryJson`.
   **Expected:** Clean error. No crash.

**Pass/Fail:** ______

---

## Section 6: Relationships & Cast

### FT-060: add_relationship between two Characters
**Priority:** High
**Tool:** `add_relationship`, `get_element`

**Steps:**
1. `add_relationship` from `CHAR_ALICE` to `CHAR_BOB` with a relationship type (e.g., "Partner" or whatever the API exposes).
   **Expected:** Success.
2. `get_element` for `CHAR_ALICE`.
   **Expected:** Relationship to Bob is listed in Alice's relationships collection.

**Pass/Fail:** ______

---

### FT-061: link_cast — add Character to Scene cast
**Priority:** High
**Tool:** `link_cast`, `get_element`

**Steps:**
1. `link_cast` linking `CHAR_BOB` to `SCENE_MID`.
   **Expected:** Success.
2. `get_element` for `SCENE_MID`.
   **Expected:** Bob appears in scene cast.

**Pass/Fail:** ______

---

## Section 7: Element Tree Mutation

### FT-070: move_element — re-parent
**Priority:** High
**Tool:** `move_element`, `list_elements`

**Steps:**
1. `move_element` on `NOTES_RES`: new parent = Story Overview (move out of `FOLDER_BACK`).
   **Expected:** Success.
2. `list_elements`.
   **Expected:** Research Notes is now a direct child of Story Overview.

**Pass/Fail:** ______

---

### FT-072: delete_element — element gone from tree
**Priority:** Critical
**Tool:** `delete_element`, `list_elements`

**Steps:**
1. `delete_element` on `WEB_LINK`.
   **Expected:** Success.
2. `list_elements`.
   **Expected:** "Research Link" no longer appears as an active node (it may appear under Trash, depending on API semantics — verify against the documented behavior).

**Pass/Fail:** ______

---

### FT-073: delete_element clears references
**Priority:** High
**Tool:** `delete_element`, `get_element`

**Steps:**
1. Note: `SCENE_OPEN` currently references `CHAR_ALICE` as ViewpointCharacter and Coffee Shop as Setting.
2. `delete_element` on `SET_COFFEE`.
   **Expected:** Success.
3. `get_element` on `SCENE_OPEN`.
   **Expected:** `Setting` is now empty/null/cleared OR points to a (none) placeholder. No stale GUID is left dangling.

**Pass/Fail:** ______

---

## Section 8: Search

### FT-080: search_text — name match
**Priority:** High
**Tool:** `search_text`

**Steps:**
1. `search_text` with query = `"Alice"`.
   **Expected:** Result includes the Alice character (and any element where the name appears, e.g., relationship references or scenes that mention Alice in a property).

**Pass/Fail:** ______

---

### FT-081: search_text — property match
**Priority:** Medium
**Tool:** `search_text`

**Steps:**
1. `search_text` with query = `"determined detective"` (text from FT-045).
   **Expected:** Alice character is returned, indicating the search reaches into Description fields.

**Pass/Fail:** ______

---

### FT-082: search_text — no results
**Priority:** Low
**Tool:** `search_text`

**Steps:**
1. `search_text` with query = `"ZZZ_NONEXISTENT_TOKEN_XYZ"`.
   **Expected:** Empty result set, no error.

**Pass/Fail:** ______

---

## Section 9: Reference Data (Read-Only)

### FT-090: get_master_plots
**Priority:** High
**Tool:** `get_master_plots`

**Steps:**
1. Call `get_master_plots`.
   **Expected:** Non-empty list including known entries (e.g., "The Quest").

**Pass/Fail:** ______

---

### FT-091: get_conflict_categories
**Priority:** Medium
**Tool:** `get_conflict_categories`

**Steps:**
1. Call `get_conflict_categories`.
   **Expected:** Non-empty list (e.g., "Person vs Person", "Person vs Nature").

**Pass/Fail:** ______

---

### FT-092: get_key_questions
**Priority:** Medium
**Tool:** `get_key_questions`

**Steps:**
1. Call `get_key_questions` with element type = Character (or whatever filter the API exposes).
   **Expected:** Non-empty list of key questions appropriate to the element type.

**Pass/Fail:** ______

---

### FT-093: get_stock_scenes
**Priority:** Medium
**Tool:** `get_stock_scenes`

**Steps:**
1. Call `get_stock_scenes`.
   **Expected:** Non-empty list of stock scene categories/entries.

**Pass/Fail:** ______

---

### FT-094: get_examples
**Priority:** Low
**Tool:** `get_examples`

**Steps:**
1. Call `get_examples`.
   **Expected:** Non-empty list of example outlines (or example data per the API's contract).

**Pass/Fail:** ______

---

### FT-095: get_beat_sheets
**Priority:** High
**Tool:** `get_beat_sheets`

**Steps:**
1. Call `get_beat_sheets`.
   **Expected:** Non-empty list including built-in templates (e.g., "Save the Cat", "Hero's Journey", "Seven Point").

**Pass/Fail:** ______

---

## Section 10: Beat Operations on a Problem

All tests in this section operate on `PROB_MAIN`. Beats are scoped to the parent Problem.

### FT-100: create_beat
**Priority:** Critical
**Tool:** `create_beat`, `get_problem_structure`

**Steps:**
1. `create_beat` on `PROB_MAIN` with title = "Inciting Incident", description = "Catalyst forces Alice into action".
   **Expected:** Success; returns a beat id. Record as `BEAT_INC`.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** The new beat appears in the beat list.

**Pass/Fail:** ______

---

### FT-101: create more beats
**Priority:** High
**Tool:** `create_beat`

**Steps:**
1. `create_beat` on `PROB_MAIN` with title = "Midpoint". Record as `BEAT_MID`.
2. `create_beat` on `PROB_MAIN` with title = "Climax". Record as `BEAT_CLI`.
   **Expected:** Both succeed; `get_problem_structure` shows three beats in order.

**Pass/Fail:** ______

---

### FT-102: update_beat — rename and edit description
**Priority:** High
**Tool:** `update_beat`, `get_problem_structure`

**Steps:**
1. `update_beat` on `BEAT_MID`: set title = "Midpoint Reversal", description = "Alice's plan unravels".
   **Expected:** Success.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** Beat shows updated title and description.

**Pass/Fail:** ______

---

### FT-103: move_beat — reorder
**Priority:** High
**Tool:** `move_beat`, `get_problem_structure`

**Steps:**
1. `move_beat` on `BEAT_CLI` to position 1 (move Climax to top).
   **Expected:** Success.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** Beat order is Climax, Inciting Incident, Midpoint Reversal.
3. Move back: `move_beat` on `BEAT_CLI` to last position.
   **Expected:** Success; order restored to Inciting Incident, Midpoint Reversal, Climax.

**Pass/Fail:** ______

---

### FT-104: assign_beat — Scene
**Priority:** Critical
**Tool:** `assign_beat`, `get_problem_structure`

**Steps:**
1. `assign_beat` on `BEAT_INC` with element = `SCENE_OPEN`.
   **Expected:** Success.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** `BEAT_INC` shows Scene assignment to Opening Scene.

**Pass/Fail:** ______

---

### FT-105: assign_beat — Problem
**Priority:** Critical
**Tool:** `assign_beat`, `get_problem_structure`

**Steps:**
1. `assign_beat` on `BEAT_MID` with element = `PROB_SUB`.
   **Expected:** Success.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** `BEAT_MID` shows Problem assignment to Sub Problem A.

**Pass/Fail:** ______

---

### FT-106: assign same Scene to multiple beats (allowed)
**Priority:** Medium
**Tool:** `assign_beat`

**Steps:**
1. `assign_beat` on `BEAT_CLI` with element = `SCENE_OPEN` (same scene already on `BEAT_INC`).
   **Expected:** Success — scenes are explicitly allowed to appear on multiple beats.
2. `get_problem_structure`.
   **Expected:** Both `BEAT_INC` and `BEAT_CLI` show Opening Scene.

**Pass/Fail:** ______

---

### FT-107: clear_beat
**Priority:** Critical
**Tool:** `clear_beat`, `get_problem_structure`

**Steps:**
1. `clear_beat` on `BEAT_CLI`.
   **Expected:** Success.
2. `get_problem_structure`.
   **Expected:** `BEAT_CLI` shows no assignment. `BEAT_INC` still shows Opening Scene (unchanged).

**Pass/Fail:** ______

---

### FT-108: delete_beat — assigned beat
**Priority:** High
**Tool:** `delete_beat`, `get_problem_structure`

**Steps:**
1. `delete_beat` on `BEAT_MID` (currently assigned to Sub Problem A).
   **Expected:** Success.
2. `get_problem_structure`.
   **Expected:** `BEAT_MID` is gone. `PROB_SUB` is no longer referenced from any beat on `PROB_MAIN`.

**Pass/Fail:** ______

---

## Section 11: Beat Sheets

### FT-110: apply_beat_sheet — Save the Cat (replaces existing)
**Priority:** Critical
**Tool:** `apply_beat_sheet`, `get_problem_structure`

**Steps:**
1. `apply_beat_sheet` on `PROB_SUB` with template = "Save the Cat".
   **Expected:** Success.
2. `get_problem_structure` for `PROB_SUB`.
   **Expected:** Beat list has 15 standard Save the Cat beats (Opening Image, Theme Stated, Set-Up, Catalyst, Debate, Break into Two, B Story, Fun and Games, Midpoint, Bad Guys Close In, All is Lost, Dark Night of the Soul, Break into Three, Finale, Final Image).

**Pass/Fail:** ______

---

### FT-111: save_beat_sheet — round-trip to .stbeat file
**Priority:** High
**Tool:** `save_beat_sheet`

**Steps:**
1. Modify the beat sheet on `PROB_SUB`: rename one beat and create one new beat (use `update_beat` and `create_beat`).
2. `save_beat_sheet` for `PROB_SUB` with path = `/tmp/mcp-full/CustomSheet.stbeat`.
   **Expected:** Success; file exists on disk.
3. Verify the file is non-empty JSON containing the beat titles and descriptions.
   **Expected:** Human-readable JSON; modifications present.

**Pass/Fail:** ______

---

### FT-112: load_beat_sheet — apply saved file to a different Problem
**Priority:** High
**Tool:** `load_beat_sheet`, `get_problem_structure`

**Steps:**
1. `load_beat_sheet` on `PROB_MAIN` with path = `/tmp/mcp-full/CustomSheet.stbeat`. (This will replace any existing beats on `PROB_MAIN` — confirm semantics with the API contract.)
   **Expected:** Success.
2. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** Beat list matches the saved custom sheet (renamed beat present, custom beat present).

**Pass/Fail:** ______

---

## Section 12: Structure Reads

### FT-120: get_structure on the outline
**Priority:** High
**Tool:** `get_structure`

**Steps:**
1. Call `get_structure`.
   **Expected:** Returns a hierarchical representation of the outline. Shape will depend on the API contract — verify it includes the Story Overview root and active children.

**Pass/Fail:** ______

---

### FT-121: get_problem_structure on PROB_MAIN
**Priority:** High
**Tool:** `get_problem_structure`

**Steps:**
1. Call `get_problem_structure` for `PROB_MAIN`.
   **Expected:** Returns the problem's beat list with assignments. Recurses into sub-problems referenced via beat assignments (per `Beat_Sheet_Test_Plan.md` RP-002 behavior).

**Pass/Fail:** ______

---

## Section 13: Persistence Round-Trip

### FT-130: Save and close
**Priority:** Critical
**Tool:** `save_outline`, `close_outline`

**Steps:**
1. `save_outline`.
   **Expected:** Success.
2. `close_outline`.
   **Expected:** Success.

**Pass/Fail:** ______

---

### FT-131: Reopen and verify all elements
**Priority:** Critical
**Tool:** `open_outline`, `list_elements`

**Steps:**
1. `open_outline` with `path = /tmp/mcp-full/FullTest.stbx`.
   **Expected:** Success.
2. `list_elements`.
   **Expected:** All surviving elements from earlier sections are present (Alice, Bob, Main Conflict, Sub Problem A, Opening Scene, Midpoint Confrontation, Backstory, Act One, Research Notes, Test World). Deleted elements (Coffee Shop, Research Link) are NOT in the active tree.

**Pass/Fail:** ______

---

### FT-132: Verify element properties survived
**Priority:** Critical
**Tool:** `get_element`

**Steps:**
1. `get_element` for Alice.
   **Expected:** `Role = "Protagonist"`, `Age = "32"`, `ElementDescription` text from FT-045 still set.
2. `get_element` for Main Conflict.
   **Expected:** `ElementDescription = "Will Alice succeed?"`, `ConflictType = "Person vs. Person"`.
3. `get_element` for Opening Scene.
   **Expected:** `ViewpointCharacter` resolves to Alice. `Setting` is empty/null (was cleared in FT-073 when Coffee Shop was deleted).
4. `get_element` for `WORLD_MAIN` (Test World).
   **Expected:** `PhysicalWorlds` contains the surviving Mars entry from FT-053 (with the updated geography from FT-052). `Cultures` contains the Northerners entry from FT-051. `Species` contains the Humans entry from FT-051. All indexed list mutations round-tripped through save/close/reopen.

**Pass/Fail:** ______

---

### FT-133: Verify beat sheets survived
**Priority:** Critical
**Tool:** `get_problem_structure`

**Steps:**
1. `get_problem_structure` for `PROB_MAIN`.
   **Expected:** Custom sheet from FT-112 is loaded with the renamed and added beats.
2. `get_problem_structure` for `PROB_SUB`.
   **Expected:** Modified Save the Cat sheet from FT-111 is loaded.

**Pass/Fail:** ______

---

### FT-134: Verify relationships and cast survived
**Priority:** High
**Tool:** `get_element`

**Steps:**
1. `get_element` for Alice.
   **Expected:** Relationship to Bob (from FT-060) is present.
2. `get_element` for Midpoint Confrontation (`SCENE_MID`).
   **Expected:** Bob is in the scene cast (from FT-061).

**Pass/Fail:** ______

---

## Section 14: Error & Edge Cases

### FT-140: Operation with no outline open
**Priority:** High
**Tool:** `list_elements` (after `close_outline`)

**Steps:**
1. `close_outline`.
   **Expected:** Success.
2. `list_elements`.
   **Expected:** Error response indicating no outline is open. No crash. Error is well-formed.
3. Reopen for subsequent tests: `open_outline` with the test path.
   **Expected:** Success.

**Pass/Fail:** ______

---

### FT-141: Open an already-open outline
**Priority:** Medium
**Tool:** `open_outline`

**Steps:**
1. With an outline already open, call `open_outline` again with a different path (e.g., `/tmp/mcp-full/Other.stbx`).
   **Expected:** Either an error indicating an outline is already open, or the existing outline is closed and the new one opens. Document which behavior occurs — both are valid contracts depending on API design. No crash either way.

**Pass/Fail:** ______

---

### FT-142: get_element with invalid GUID
**Priority:** High
**Tool:** `get_element`

**Steps:**
1. `get_element` with GUID = `00000000-0000-0000-0000-000000000000`.
   **Expected:** Error response indicating element not found, or a (none) placeholder per the documented behavior. No crash.

**Pass/Fail:** ______

---

### FT-143: update_property with unknown property name
**Priority:** Medium
**Tool:** `update_property`

**Steps:**
1. `update_property` on `CHAR_ALICE` with property = `NotARealField`, value = `"x"`.
   **Expected:** Error response indicating the property is not valid for that element type. No silent success. No crash.

**Pass/Fail:** ______

---

### FT-144: update_property with wrong type for valid property
**Priority:** Medium
**Tool:** `update_property`

**Steps:**
1. `update_property` on `SCENE_OPEN`: set `ViewpointCharacter = "not a guid"`.
   **Expected:** Error response indicating the value is not a valid Character reference. No crash.

**Pass/Fail:** ______

---

### FT-145: delete_element on Story Overview root
**Priority:** Medium
**Tool:** `delete_element`

**Steps:**
1. Identify the Story Overview root GUID.
2. `delete_element` on the Story Overview root.
   **Expected:** Error indicating the root cannot be deleted. No crash. Outline remains usable.

**Pass/Fail:** ______

---

### FT-146: move_element to its own descendant
**Priority:** Low
**Tool:** `move_element`

**Steps:**
1. `move_element` on `PROB_MAIN`: new parent = `PROB_SUB` (a child of `PROB_MAIN`).
   **Expected:** Error indicating cycles are not allowed. No crash. Tree state unchanged.

**Pass/Fail:** ______

---

### FT-147: assign_beat with cross-problem element
**Priority:** Low
**Tool:** `assign_beat`

**Steps:**
1. Pick a beat on `PROB_MAIN`.
2. `assign_beat` with element = `PROB_MAIN` itself (a problem cannot be assigned to its own beat sheet, per `Beat_Sheet_Test_Plan.md` BA-009).
   **Expected:** Error or rejection. No crash.

**Pass/Fail:** ______

---

### FT-148: save_outline with no path set after fresh blank open
**Priority:** Low
**Tool:** `save_outline`

**Steps:**
1. `close_outline`. `open_outline` with a brand-new path that does not yet exist (creates a new in-memory outline). Add one element. `save_outline` without arguments.
   **Expected:** Behavior depends on contract — either saves to the path passed at open time, or returns an error asking for a path. No crash. Document the observed behavior.

**Pass/Fail:** ______

---

### FT-149: Clean close at end
**Priority:** Critical
**Tool:** `close_outline`

**Steps:**
1. `save_outline`.
   **Expected:** Success.
2. `close_outline`.
   **Expected:** Success, no errors.

**Pass/Fail:** ______

---

## Test Results Summary

| Section | Tests | Passed | Failed | Skipped |
|---------|-------|--------|--------|---------|
| 1. Outline Lifecycle | FT-001 to FT-005 | | | |
| 2. Element Creation | FT-010 to FT-021 | | | |
| 3. Element Listing & Inspection | FT-030 to FT-036 | | | |
| 4. Property Updates | FT-040 to FT-046 | | | |
| 5. Collection Entries (StoryWorld) | FT-050 to FT-054 | | | |
| 6. Relationships & Cast | FT-060 to FT-061 | | | |
| 7. Element Tree Mutation | FT-070, FT-072, FT-073 | | | |
| 8. Search | FT-080 to FT-082 | | | |
| 9. Reference Data | FT-090 to FT-095 | | | |
| 10. Beat Operations | FT-100 to FT-108 | | | |
| 11. Beat Sheets | FT-110 to FT-112 | | | |
| 12. Structure Reads | FT-120 to FT-121 | | | |
| 13. Persistence Round-Trip | FT-130 to FT-134 | | | |
| 14. Error & Edge Cases | FT-140 to FT-149 | | | |
| **TOTAL** | **70** | | | |

---

## FULL TEST RESULT: PASS / FAIL

**Critical Issues**: ________________

**Blocking Issues** (must fix before release): ________________

**Non-blocking Issues** (can ship with known issues): ________________

**Tested by**: _________ **Date**: _________ **Build**: _________
