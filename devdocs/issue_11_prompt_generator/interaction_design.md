# Prompt Generator Interaction Design

This document maps StoryCAD data to user interaction patterns identified from research on existing prompt generators.

**Related:**
- [`data_inventory.md`](data_inventory.md) - Complete data catalog
- [`status_log.md`](status_log.md) - Project status

---

## Interaction Patterns from Research

| Pattern | Example Apps | User Effort | Engagement |
|---------|--------------|-------------|------------|
| **Simple Random** | sharkham/prompt-generator | None (1 click) | Quick, low friction |
| **Category Selection** | Reedsy, ServiceScape | Low (1-2 choices) | Targeted results |
| **Template-Based** | Plot-Generator.org.uk | Medium (fill blanks) | Personalized |
| **Browsing/Exploration** | garykac/plotto | High (navigate) | Deep, educational |

---

## Pattern 1: Simple Random (One-Click Generate)

### Interaction
- Single "Generate Prompt" button
- No user input required
- Instant result

### Data Mapping

| Output Field | Data Source | Count |
|--------------|-------------|------:|
| Genre | Lists.json → Genre | 18 |
| Theme | Lists.json → Theme | 33 |
| Tone | Lists.json → Tone | 219 |
| Dramatic Situation | Tools.json → DramaticSituations | 36 |
| Conflict Example | Controls.json → random example | 279 |

### Sample Output
```
Genre: Mystery
Theme: Betrayal
Tone: Suspenseful
Situation: Supplication
Conflict: Your boss asks you to delete data on a failed project
```

### Lead Gen Value: HIGH
- Instant gratification
- Low barrier to entry
- Easy to share results

---

## Pattern 2: Category Selection (Filter then Generate)

### Interaction
- User selects one category/filter
- Click generate for random within that category
- Option to regenerate or change filter

### Data Mapping

| User Selects | Then Randomize From | Options |
|--------------|---------------------|--------:|
| **Prompt Profile** | Fields for that profile | 4 |
| **Genre** | Theme, Tone, situations | 18 |
| **Stock Scene Category** | Scenes in that category | 11 |
| **Conflict Category** | Subcategories → Examples | 8 |

### Prompt Profiles

| Profile | Fields Used |
|---------|-------------|
| **Minimal** | Genre, Theme, Tone, Dramatic Situation |
| **Character-First** | Archetype, Role, Trait, Wound, Motive |
| **Problem-First** | Conflict Category/Sub/Example, Goal, Opposition |
| **World-First** | Genre, WorldType, Locale, Season, Tone |

### Lead Gen Value: MEDIUM
- Still low friction
- User feels more control
- Better targeting for specific needs

---

## Pattern 3: Template-Based (Slot Filling)

### Interaction
- Display template with blanks
- Auto-fill with random values OR
- User clicks individual slots to randomize/change

### Template Examples

**Character Seed Template:**
```
A [Role] with [Trait] and [Wound] must [Goal] while facing [ConflictExample].
```
Combinations: 215 × 158 × 121 × 70 × 279 = **80 billion**

**Story Concept Template:**
```
In a [Genre] story about [Theme], the [Archetype] confronts [DramaticSituation].
```
Combinations: 18 × 33 × 9 × 36 = **192,456**

**Scene Starter Template:**
```
[StockScene] — but [TwistOrTurn].
```
Combinations: 347 × 35 = **12,145**

### UI Features
- Click any field to regenerate just that value
- Lock icon to keep a value while regenerating others
- "Shuffle All" button for complete regeneration

### Lead Gen Value: MEDIUM-HIGH
- More engaging interaction
- Users invest more time
- Higher perceived value

---

## Pattern 4: Browsing/Exploration (Plotto-style)

### Interaction
- Hierarchical navigation
- Click to drill down
- Related items suggested

### Data Mapping

| Browsable Structure | Navigation Flow |
|---------------------|-----------------|
| **Dramatic Situations** | List → Situation → Roles → Related |
| **Master Plots** | List → Plot → Scene Breakdown → Details |
| **Conflict Hierarchy** | Category → Subcategory → Examples |
| **Stock Scenes** | Category → Scene List → Scene Details |

### Example Flow: Dramatic Situations
1. Browse 36 Dramatic Situations
2. Click "Supplication"
3. See roles: Persecutor, Supplicant, Judge
4. Option: Generate character seeds for each role
5. Option: Suggest related stock scenes

### Lead Gen Value: LOWER
- Requires more user investment
- Appeals to serious writers
- Educational value (teaches craft)
- Good for retention, less for acquisition

---

## Recommended MVP Approach

### Phase 1: One-Click + Profile Selection
1. **Default view:** One-click random (Pattern 1)
2. **Profile tabs:** Minimal, Character-First, Problem-First, World-First (Pattern 2)
3. **Regenerate button** for each field (Pattern 3 lite)

### Phase 2: Enhanced Interaction
1. **Lock/unlock fields** (Pattern 3 full)
2. **Category drill-down** for advanced users (Pattern 2 extended)

### Phase 3: Exploration Mode
1. **Browse Dramatic Situations** (Pattern 4)
2. **Browse Master Plots** (Pattern 4)
3. **Educational content** about each element

---

## UI Wireframe Concept (MVP)

```
┌─────────────────────────────────────────────────────┐
│  StoryCAD Prompt Generator                          │
├─────────────────────────────────────────────────────┤
│  [Minimal] [Character] [Problem] [World]            │  ← Profile tabs
├─────────────────────────────────────────────────────┤
│                                                     │
│  Genre:     [ Mystery          ] [↻]                │  ← Click ↻ to regenerate
│  Theme:     [ Betrayal         ] [↻]                │
│  Tone:      [ Suspenseful      ] [↻]                │
│  Situation: [ Supplication     ] [↻]                │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │ A persecutor threatens a supplicant who     │   │  ← Generated prompt
│  │ must appeal to a higher authority...        │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│         [ Generate New Prompt ]                     │  ← Main action
│                                                     │
│  ─────────────────────────────────────────────────  │
│  💡 Want to develop this into a full story?        │
│     [ Try StoryCAD Free → ]                        │  ← Lead capture CTA
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Next Steps

- [ ] Finalize MVP field selection for each profile
- [ ] Design lead capture integration (email gate? post-generation CTA?)
- [ ] Create UI mockups in Divi
- [ ] Implement JavaScript prompt generation logic
- [ ] Test with users
