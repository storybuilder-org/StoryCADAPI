# Outliner End-to-End Manual Test
**Time**: ~10–15 minutes (depends on LLM round-trip)
**Purpose**: Verify the Outliner sample produces a usable `.stbx` outline from a real prose file, against a live LLM. Replaces the automated `EndToEndOutlineTests` (removed — required a live key and non-deterministic outputs).

## Prerequisites
- Fresh build of the Outliner sample (`samples/Outliner/Outliner`).
- `OPENAI_API_KEY` set in the environment (and optionally `OPENAI_MODEL`, default `gpt-4o`).
- A short prose file (`.docx`, `.pdf`, or `.txt`) — a short story is enough; novellas can take minutes.
- StoryCAD installed and openable, for `.stbx` round-trip verification.

---

### OE-001: Launch Outliner
**Priority:** Critical
**Time:** ~1 minute

**Steps:**
1. Launch the Outliner sample.
   **Expected:** Application window opens; NavigationView shows Single and Batch entries; no error dialogs.

2. Click the Single entry.
   **Expected:** ContentPage appears with a "Read Story" button, an empty `Story Content` text area, and a status TextBlock.

**Pass/Fail:** ______

---

### OE-002: Read Prose File
**Priority:** Critical
**Time:** ~1 minute

**Steps:**
1. Click "Read Story" and pick the prose file.
   **Expected:** File picker opens, accepts `.docx` / `.pdf` / `.txt`.

2. Confirm the prose loads into the `Story Content` text area.
   **Expected:** Prose text visible in the text area (not the status accumulator). Status TextBlock shows a "Loaded: filename" message in gray.

**Pass/Fail:** ______

---

### OE-003: Run Outline Generation
**Priority:** Critical
**Time:** ~5–10 minutes for a short story (LLM round-trip dominates)

**Steps:**
1. Click "Generate Outline" (or equivalent submit control).
   **Expected:** Status TextBlock updates progressively ("Analyzing with LLM...", "Building outline..."). No unhandled exception.

2. Wait for completion.
   **Expected:** Status reaches a "complete" / "saved" message. Output path shown to the user.

**Pass/Fail:** ______

---

### OE-004: Per-Run Artifacts Written
**Priority:** Critical
**Time:** ~1 minute

**Steps:**
1. Open the output folder shown at the end of OE-003.
   **Expected:** Four files present, named `<input>.stbx`, `<input>.raw.json`, `<input>.costs.json`, `<input>.rating.json`.

2. Open `<input>.costs.json`.
   **Expected:** Valid JSON with model id, prompt tokens, completion tokens, total cost. Non-zero values.

3. Open `<input>.rating.json`.
   **Expected:** Valid JSON with an auto completeness score and a thumbs verdict.

4. Confirm `<input>.raw.json` is non-empty and parses as JSON.
   **Expected:** Valid JSON, contents look like the schema (`story_overview`, `characters`, `settings`, `scenes`, `problems`).

**Pass/Fail:** ______

---

### OE-005: Open Generated `.stbx` in StoryCAD
**Priority:** Critical
**Time:** ~2 minutes

**Steps:**
1. Launch StoryCAD.

2. File > Open Story and select `<input>.stbx` from OE-004.
   **Expected:** File opens. No warning or error dialogs about format, schema, or missing elements.

3. Confirm StoryExplorer tree shows the outline.
   **Expected:** Tree shows Story Overview at root, with Character(s), Setting(s), Problem(s), Scene(s) as expected from the prose.

**Pass/Fail:** ______

---

### OE-006: Spot-Check Outline Content
**Priority:** Critical
**Time:** ~3 minutes

**Steps:**
1. Click Story Overview.
   **Expected:** Title, Author, Premise populated. StoryType and StoryGenre populated. StoryProblem points at a real Problem element.

2. Click each Character.
   **Expected:** Role, Age, Sex, Appearance populated. The "Character Sketch" field on the Character page is populated (not just Notes).

3. Click each Setting.
   **Expected:** Locale and one or more sensory fields (Sights, Sounds, Lighting) populated. The "Setting Summary" field is populated (not just Notes).

4. Click each Problem.
   **Expected:** ConflictType, Protagonist, Antagonist, Outcome populated. The "Story Question" field is populated (not just Notes). All six goal/motive/conflict slots (ProtGoal/ProtMotive/ProtConflict + AntagGoal/AntagMotive/AntagConflict) populated.

5. Click each Scene.
   **Expected:** ViewpointCharacter, Setting, CastMembers, Protagonist, Antagonist populated. The "Scene Sketch" field is populated (not just Notes).

**Pass/Fail:** ______

---

### OE-007: Error Path — Missing API Key
**Priority:** High
**Time:** ~2 minutes

**Steps:**
1. Close Outliner. Unset `OPENAI_API_KEY` (or rename it temporarily).

2. Relaunch Outliner, read the same prose file, click "Generate Outline".
   **Expected:** A user-visible error message naming the missing key (not a stack trace, not a silent failure). Status TextBlock surfaces the problem.

3. Restore the key for subsequent runs.

**Pass/Fail:** ______

---

## END-TO-END RESULT: PASS / FAIL

**Notes**:
_____________________

**Tested by**: _________ **Date**: _________ **Build**: _________
