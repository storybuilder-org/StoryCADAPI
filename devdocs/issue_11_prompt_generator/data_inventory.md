# StoryCAD Data Inventory for Prompt Generator

This document catalogs all available data from StoryCAD that can be used for prompt generation.

**Source files:** `StoryCADLib/Assets/Install/`
- `Lists.json` - Simple key→array lists
- `Tools.json` - Structured writing tools data
- `Controls.json` - Conflict builder data

**API access:** `StoryCADApi` provides `ListData`, `ToolsData`, `ControlData`

---

## Lists.json (75 categories, ~2,600+ values)

### Large Lists (50+ values)

| Category | Count | Description |
|----------|------:|-------------|
| Nationality | 244 | Character nationalities |
| Tone | 219 | Story/scene tone options |
| Role | 215 | Character roles |
| ProblemSubject | 212 | Story problem subjects |
| ValueExchange | 161 | Value-based interactions |
| Trait | 158 | Character traits |
| Behavior | 157 | Character behaviors |
| Wound | 121 | Character emotional wounds |
| Habit | 101 | Character habits |
| Motive | 96 | Character motivations |
| Locale | 90 | Setting locations |
| Goal | 70 | Character goals |
| Skill | 57 | Character skills |

### Medium Lists (15-49 values)

| Category | Count | Description |
|----------|------:|-------------|
| Emotion | 45 | Emotional states |
| Values | 38 | Character values |
| Theme | 33 | Story themes |
| LiteraryStyle | 27 | Writing styles |
| Method | 27 | Methods/approaches |
| LiteraryTechnique | 25 | Writing techniques |
| Attitude | 20 | Character attitudes |
| Genre | 18 | Story genres |
| Confidence | 17 | Confidence levels |
| Shrewdness | 16 | Shrewdness spectrum |
| SceneType | 15 | Types of scenes |

### Small Lists (5-14 values)

| Category | Count | Description |
|----------|------:|-------------|
| Conflict | 14 | Conflict types |
| Sociability | 14 | Social tendencies |
| Assurance | 13 | Assurance levels |
| Aggression | 12 | Aggression spectrum |
| SceneOutcome | 12 | Scene outcome types |
| Adventurousness | 11 | Adventure tendency |
| Conscientiousness | 11 | Conscientiousness spectrum |
| Creativity | 11 | Creativity levels |
| Hair | 11 | Hair descriptions |
| Outcome | 11 | Story outcomes |
| Title | 11 | Title/honorific options |
| Dominance | 10 | Dominance spectrum |
| Enthusiasm | 10 | Enthusiasm levels |
| Abnormality | 9 | Abnormality types |
| Archetype | 9 | Character archetypes |
| Dynamic | 9 | Character dynamics |
| Enneagram | 9 | Enneagram types |
| Focus | 9 | Focus areas |
| Intelligence | 9 | Intelligence spectrum |
| LanguageStyle | 9 | Language style options |
| ScenePurpose | 9 | Scene purposes |
| Sensitivity | 9 | Sensitivity levels |
| Topic | 9 | Topic categories |

### Tiny Lists (1-8 values)

| Category | Count | Description |
|----------|------:|-------------|
| Stability | 8 | Stability spectrum |
| StoryType | 8 | Story type options |
| WorldType | 8 | World type options |
| WoundCategory | 8 | Wound categories |
| Build | 7 | Character build types |
| Complexion | 7 | Complexion types |
| ConflictSource | 7 | Conflict sources |
| ProblemSource | 7 | Problem sources |
| Season | 7 | Seasons |
| Voice | 7 | Voice options |
| ConflictType | 7 | Conflict type categories |
| Race | 6 | Character race options |
| StoryRole | 6 | Story role types |
| Eyes | 5 | Eye descriptions |
| Ontology | 5 | Ontology options |
| Tense | 5 | Narrative tense |
| ToneLogic | 5 | Tone logic options |
| Viewpoint | 5 | POV options |
| WorldRelation | 5 | World relation types |
| AgencySource | 4 | Agency sources |
| Font | 4 | Font options |
| ProblemCategory | 4 | Problem categories |
| RuleTransparency | 4 | Rule transparency levels |
| SystemType | 4 | System types |
| ProblemType | 3 | Problem types |
| ScaleOfDifference | 3 | Scale of difference |

---

## Tools.json (Structured Writing Tools)

### Stock Scenes (347 total across 11 categories)

| Category | Description |
|----------|-------------|
| Western Stock Scenes | Classic western scenarios |
| Suspenseful Moments | Tension-building situations |
| Obstacles to the Chase | Chase scene complications |
| Twists and Turns | Plot twist scenarios |
| Obstacles to Discovery | Mystery complications |
| Obstacles to Romance | Romantic conflict scenarios |
| Discoveries | Revelation scenarios |
| Escapes | Escape scenarios |
| Rescues | Rescue scenarios |
| Revenge | Revenge scenarios |
| Ticking Clock | Time pressure scenarios |

### Dramatic Situations (36 - Polti's Classic Situations)

Each situation includes:
- Name
- Roles (2-4 character roles)
- Notes (description)
- Examples (optional)

Examples:
- Supplication (persecutor, supplicant, judge)
- Crimes of Love
- A Family at War (Enmity of Kinsmen)
- *(33 more...)*

### Master Plots (18)

Each includes:
- Name
- Notes (description)
- Scenes (breakdown of key scenes)

Examples:
- Adventure
- Ascent/Descent
- Discovery
- Quest
- Revenge
- Transformation
- *(12 more...)*

### Beat Sheets (11)

Story structure templates with scene breakdowns.

### Other Tools Data

| Category | Count | Description |
|----------|------:|-------------|
| Topics | 9 | Writing guidance topics |
| Key Questions | 5 | Categories of craft questions |
| Male First Names | 25 | Random name generation |
| Female First Names | 25 | Random name generation |
| Last Names | 25 | Random name generation |
| Relationships | 23 | Character relationship types |

---

## Controls.json (Conflict Builder)

### Conflict Types (8 categories, 77 subcategories, 279 examples)

| Category | Subcategories | Examples |
|----------|-------------:|--------:|
| Relationship | 6 | ~25 |
| Information | 4 | ~12 |
| Interest | 3 | ~14 |
| Structural | 4 | ~21 |
| Value | 37 | ~125 |
| Identity | 5 | ~20 |
| Criminal Activities | 10 | ~40 |
| Criminal Psychology | 4 | ~22 |

### Relation Types (65)

Character relationship options:
- Family (Father, Mother, Brother, Sister, etc.)
- Extended family (Aunt, Uncle, Cousin, etc.)
- In-laws (Father-in-law, Mother-in-law, etc.)
- Romantic (Husband, Wife, Lover, Ex-, etc.)
- Professional (Employer, Employee, Co-worker, etc.)
- Social (Friend, Neighbor, Rival, etc.)
- Educational (Teacher, Student, Mentor, etc.)

---

## Combinatorial Analysis

### Simple Prompt (5 fields)
Genre × Theme × Tone × Role × Conflict Subcategory
= 18 × 33 × 219 × 215 × 77
= **2.1 billion combinations**

### Character Seed (4 fields)
Archetype × Trait × Wound × Motive
= 9 × 158 × 121 × 96
= **16.5 million combinations**

### Situation Hook (3 fields)
Dramatic Situation × Stock Scene × Conflict Example
= 36 × 347 × 279
= **3.5 million combinations**

---

## Recommended Fields for Prompt Profiles

### Minimal Profile
- Genre (18)
- Theme (33)
- Tone (219)
- Dramatic Situation (36)

### Character-First Profile
- Archetype (9)
- Role (215)
- Trait (158)
- Wound (121)
- Motive (96)

### Problem-First Profile
- Conflict Category (8)
- Conflict Subcategory (77)
- Goal (70)
- Opposition (8)
- Outcome (11)

### World-First Profile
- Genre (18)
- WorldType (8)
- Locale (90)
- Season (7)
- Tone (219)
