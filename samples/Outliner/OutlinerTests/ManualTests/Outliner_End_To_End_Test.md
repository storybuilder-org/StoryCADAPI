# Outliner End-to-End Manual Test
**Time**: ~10–15 minutes (depends on LLM round-trip)
**Purpose**: Verify the Outliner sample produces a usable `.stbx` outline from a real prose file, against a live LLM. Replaces the automated `EndToEndOutlineTests` (removed — required a live key and non-deterministic outputs).

## Prerequisites
- Fresh build of the Outliner sample (`samples/Outliner/Outliner`).
- `OPENAI_API_KEY` set in the environment (and optionally `OPENAI_MODEL`, default `gpt-4o`).
- Reference prose fixture: `samples/Outliner/OutlinerTests/ManualTests/Fixtures/the_story_of_an_hour.txt` (Kate Chopin, 1894, US public domain, ~1,050 words). Checked in so every tester runs against the same prose and the expected-output spot-checks below are meaningful.
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
1. Click "Read Story" and pick `Fixtures/the_story_of_an_hour.txt`.
   **Expected:** File picker opens, accepts `.txt`. File loads.

2. Confirm the prose loads into the `Story Content` text area.
   **Expected:** Prose text visible in the text area (not the status accumulator); opens with "Knowing that Mrs. Mallard was afflicted with a heart trouble..." and ends with "of the joy that kills." Status TextBlock shows a "Loaded: the_story_of_an_hour.txt" message in gray.

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

LLM output varies run-to-run; assertions below name *what should appear*, not exact text. If the outline differs in obvious ways (wrong protagonist, missing characters, wrong conflict type) it's a failure worth investigating.

**Steps:**
1. Click Story Overview.
   **Expected:** Title contains "Story of an Hour" or "Mrs. Mallard". Author "Kate Chopin". Premise references Mrs. Mallard, the news of her husband's death, her awakening to freedom, and her death when he returns alive. StoryType "Short-Short". StoryGenre something in the literary-fiction family ("Literary Fiction", "Drama", "Realism"). StoryProblem GUID points at the lone Problem element.

2. Click each Character. Expected characters (the LLM may also fold Richards in or drop him):
   - **Mrs. Louise Mallard** (or "Louise Mallard") — Role: Protagonist. Sex: Female. Age: young / unspecified adult. Appearance references a fair, calm face. PhysNotes references the heart condition. Character Sketch field on the Character page is populated.
   - **Brently Mallard** — Role: secondary / "husband". Sex: Male. Sketch describes him as the supposed-dead husband who returns alive.
   - **Josephine** — Louise's sister, the one who breaks the news. May be present; LLM may merge.
   - Roles, Sex, and Character Sketch fields populated (not just Notes).

3. Click each Setting.
   **Expected:** One Setting — the Mallard house (or the upstairs room with the open window). Locale populated. Sights / Sounds populated (spring trees, sparrows, peddler crying wares, distant song). The "Setting Summary" field is populated (not just Notes).

4. Click the Problem.
   **Expected:** One Problem. ConflictType "Person vs. Self" (preferred) or "Person vs. Society"; *not* "Person vs. Person" against Brently. Protagonist = Louise Mallard. Antagonist = Louise Mallard (Person vs. Self) — same GUID as Protagonist. Outcome references her death from the shock. Story Question field populated. All six goal/motive/conflict slots populated (ProtGoal/ProtMotive/ProtConflict + AntagGoal/AntagMotive/AntagConflict).

5. Click each Scene. Expected scene structure (1–3 scenes, depending on segmentation):
   - News-of-death delivered (Josephine + Richards present; Louise weeps).
   - Louise alone in the upstairs room, by the window, recognizing freedom.
   - Brently returns; Louise dies.
   - For each scene: ViewpointCharacter = Louise (except possibly the final scene if the LLM frames it externally). Setting populated. CastMembers populated. Protagonist = Louise. Scene Sketch field populated.

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
