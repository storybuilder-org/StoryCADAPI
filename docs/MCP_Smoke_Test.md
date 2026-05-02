# StoryCAD MCP Smoke Test
**Time**: ~5 minutes
**Purpose**: Verify the storycad MCP server is healthy and the StoryCADAPI surface is wired correctly. Run this before any deeper MCP testing.

## Prerequisites
- `storycad` MCP server registered and connected (`claude mcp list` shows ✓)
- A writable scratch directory (test uses `/tmp/mcp-smoke/`)
- An existing `.stbx` to seed from. `open_outline` does not create new files — copy a sample (e.g., `StoryCAD/StoryCADLib/Assets/Install/samples/Hamlet.stbx`) into the scratch dir as `SmokeTest.stbx` before running.
- No outline currently open

## Test Conventions
- Each step shows the MCP tool name and key arguments.
- "Expected" describes a successful response shape, not exact JSON.
- Stop on first failure — later steps depend on earlier state.

---

### MCP-001: Server reachable
**Priority:** Critical
**Tool:** (host listing — not a tool call)

**Steps:**
1. Confirm `storycad` appears in the MCP tool list with `mcp__storycad__*` prefixed tools.
   **Expected:** Tools include `open_outline`, `add_element`, `save_outline`, `close_outline` at minimum.

**Pass/Fail:** ______

---

### MCP-002: Create new outline
**Priority:** Critical
**Tool:** `open_outline`

**Steps:**
1. Call `open_outline` with `path = /tmp/mcp-smoke/SmokeTest.stbx` (pre-seeded from a sample).
   **Expected:** Returns success; outline is now the active model.
2. Call `list_elements`.
   **Expected:** Sample's tree returned (StoryOverview root plus its children).

**Pass/Fail:** ______

---

### MCP-003: Add Character and Scene
**Priority:** Critical
**Tool:** `add_element`

**Steps:**
1. `add_element` with `type = Character`, `name = "Test Character"`, parent = Overview.
   **Expected:** Returns the new element's GUID.
2. `add_element` with `type = Scene`, `name = "Test Scene"`, parent = Overview.
   **Expected:** Returns the new element's GUID.
3. `list_elements`.
   **Expected:** Both new elements present in tree.

**Pass/Fail:** ______

---

### MCP-004: Update property and inspect
**Priority:** Critical
**Tools:** `update_property`, `get_element`

**Steps:**
1. `update_property` on the Character: set `Role = "Protagonist"`.
   **Expected:** Success.
2. `get_element` for that Character GUID.
   **Expected:** Response shows `Role = "Protagonist"` and `Name = "Test Character"`.

**Pass/Fail:** ______

---

### MCP-005: Reference-data tool
**Priority:** High
**Tool:** `get_master_plots`

**Steps:**
1. Call `get_master_plots`.
   **Expected:** Non-empty list of master plot names returned. Confirms read-only API surface is intact.

**Pass/Fail:** ______

---

### MCP-006: Save outline
**Priority:** Critical
**Tool:** `save_outline`

**Steps:**
1. Call `save_outline`.
   **Expected:** Success; `/tmp/mcp-smoke/SmokeTest.stbx` exists on disk and is non-empty.

**Pass/Fail:** ______

---

### MCP-007: Close and reopen — state persists
**Priority:** Critical
**Tools:** `close_outline`, `open_outline`, `list_elements`

**Steps:**
1. `close_outline`.
   **Expected:** Success.
2. `open_outline` with `path = /tmp/mcp-smoke/SmokeTest.stbx`.
   **Expected:** Success.
3. `list_elements`.
   **Expected:** Test Character and Test Scene both present with prior properties (verify Character.Role = "Protagonist" via `get_element`).

**Pass/Fail:** ______

---

### MCP-008: Clean close
**Priority:** Critical
**Tool:** `close_outline`

**Steps:**
1. `close_outline`.
   **Expected:** Success, no errors.

**Pass/Fail:** ______

---

## SMOKE TEST RESULT: PASS / FAIL

**Notes**:
_____________________

**Tested by**: _________ **Date**: _________ **Build**: _________
