# Issue #61: Fiction Prompt Generator - Status Log

## Current Status
**Phase:** Research / Design
**Last Updated:** 2026-02-04

---

## Session Log

### 2026-01-26 - Issue Created & Research Consolidated
- Created issue #61 in Collaborator repo
- Set up devdocs folder structure
- Initial concept: context-aware writing prompts using story outline elements
- **Business context added**: Lead generation tool (Getting Attention recommendation)
- Deployment likely web-based from storybuilder.org (WordPress/Divi)
- Related to StoryCAD #1246 (samples and API guidance)

### 2026-01-26 - Research Files Reviewed
Reviewed user-provided research files:
- `perplexity_prompt.md` - Universal and fiction prompt generator templates
- `broad_story_prompt.md` - StoryCAD-specific Story Prompt definition and schema
- `story_element_prompt.md` - Minimal schema for generating Story Problems
- `prompt_generator_samples.md` - Integration notes and C# code samples

**Key insight from research:**
> "A Story Prompt is a *partial story state* - structural, not prose, incomplete by design."

**Design rule:**
> "Incomplete enough to invite authorship, but structured enough to imply story."

### 2026-02-04 - Interaction Design Session

**Completed:**
- Created complete data inventory (`data_inventory.md`)
  - Lists.json: 75 categories, ~2,600+ values
  - Tools.json: 347 stock scenes, 36 dramatic situations, 18 master plots
  - Controls.json: 8 conflict categories, 77 subcategories, 279 examples
  - Combinatorial space: 2.1 billion+ combinations from 5 fields
- Mapped StoryCAD data to interaction patterns (`interaction_design.md`)
  - Pattern 1: Simple Random (one-click)
  - Pattern 2: Category Selection (profile tabs)
  - Pattern 3: Template-Based (slot filling with regenerate)
  - Pattern 4: Browsing/Exploration (Plotto-style drill-down)
- Recommended MVP approach: One-click + Profile Selection + Per-field regenerate

**Documents created this session:**
- `data_inventory.md` - Complete catalog of available data with counts
- `interaction_design.md` - Interaction patterns mapped to data, UI wireframe

---

### 2026-02-04 - Architectural Pivot: Reconsidering Divi

**Pain point identified:** Calling C# StoryCADApi from Divi (WordPress/PHP) requires an intermediary service. This adds complexity.

**Alternative:** .NET website instead of Divi

| Aspect | Divi (WordPress) | .NET Website |
|--------|------------------|--------------|
| API access | Needs intermediary | Direct (StoryCADApi) |
| Data access | Static JSON export | Native StoryCADLib |
| AI enhancement | Cloudflare Worker | Built-in (Collaborator) |
| Lead capture | Divi forms (easy) | Custom implementation |
| Hosting | SiteGround (included) | Azure, VPS, etc. |
| UI development | Visual builder | Code (Blazor, Razor, etc.) |

**Advantages of .NET:**
- Direct API access - no bridging problem
- Reference StoryCADLib as NuGet package
- AI enhancement (Phase 2) trivial - just use Collaborator
- Consistent tech stack with StoryCAD
- Website itself becomes an API sample (#1246 alignment)

**Decision: Reconsidering Divi in favor of .NET website.**

---

### 2026-02-04 - Deployment Refinement: GitHub Pages + Blazor WASM

**Research:** Investigated [pollydocs.org](https://www.pollydocs.org/) (referenced in #1246)
- Uses **docfx** (.NET documentation generator)
- Hosted on **GitHub Pages** (free)
- Custom domain via CNAME
- Deployed via GitHub Actions

**Key insight:** Same model works for StoryCAD API-Samples:
- API-Samples repo is planned to go public (per #1246)
- GitHub Pages becomes free hosting option
- Can host both docfx docs AND Blazor WASM interactive app

**Likely deployment approach:**

| Component | Technology | Hosting |
|-----------|------------|---------|
| API documentation | docfx | GitHub Pages |
| Prompt Generator | Blazor WASM | GitHub Pages (same site) |
| Build/Deploy | GitHub Actions | Free |
| Domain | Custom (optional) | ~$12/year |

**Why this works:**
- **Free hosting** (GitHub Pages)
- **Direct StoryCADLib access** (Blazor WASM runs .NET in browser)
- **Consistent tech stack** (.NET throughout)
- **Follows Polly model** (proven approach)
- **API-Samples becomes a live demo** (aligns with #1246)

**Cost: Free** (except optional custom domain)

---

### RESUME POINT

**Deployment approach selected:** GitHub Pages + Blazor WASM on public API-Samples repo

**Next steps:**
1. Coordinate with #1246 to make API-Samples repo public
2. Set up docfx for API documentation
3. Create Blazor WASM prompt generator project
4. Configure GitHub Actions for build/deploy
5. Finalize MVP field selection for each profile
6. Design lead capture integration (custom form in Blazor)
7. Implement prompt generation logic

**Related files to review when resuming:**
- `interaction_design.md` - UI wireframe and pattern recommendations
- `data_inventory.md` - Recommended fields per profile
- `broad_story_prompt.md` - Original prompt schema design
- `blazor_github_pages_deployment.md` - Deployment guide with configuration and gotchas

---

### 2026-02-04 - Architecture & Deployment Research

**Context provided by user:**
- 3-site SiteGround/Divi hosting package (1 used, 1 reserved for storybuilder.org, 1 available)
- Available site could host API/samples website (related to StoryCAD #1246)
- Prompt Generator could be part of that website

**Architectural Decision: AI vs Deterministic vs Hybrid**

Reviewed all devdocs files. Key finding from `broad_story_prompt.md` Section 5.2:
> "List-Driven Generation - Favor selection from existing StoryCAD lists. AI fills gaps only after list choices are made."

This is a **hybrid approach** already documented in research:
1. **Deterministic base**: User selects from StoryCAD lists, combinatorial assembly
2. **Optional AI enhancement**: LLM fills gaps after list choices made

**Implications:**
- Deterministic-only version can be pure static site (no backend, no LLM costs)
- AI enhancement could be optional/premium feature (adds cost, needs backend)
- Hybrid supports tiered deployment: free deterministic → paid AI-enhanced

**StoryCAD Data Sources for Deterministic Generation:**
- Master Plots (Quest, Revenge, Transformation, etc.)
- Dramatic Situations (36 classic from Polti)
- Stock Scenes (common scene patterns)
- ToolsData (conflict builder, trait spectrums, relationships)
- Lists: Genres, Themes, Archetypes, Conflict types, Traits

**See [`data_inventory.md`](data_inventory.md) for complete catalog with counts.**

**Cross-Reference: StoryCAD #1246**

Prompt Generator is **Sample #9** in #1246 sample list:
> "Generate prompts from Master Plots, Dramatic Situations, Stock Scenes"

Issue #1246 has open decision **D2: Documentation Hosting** with same options:
| Option | Example | Pros | Cons |
|--------|---------|------|------|
| Add to storybuilder.org | - | Single brand | Different audience |
| Add to user manual | GitHub Pages | Already exists | Mixes audiences |
| **Dedicated site** | pollydocs.org | Clean separation | More to maintain |
| README-driven | GitHub markdown | Simplest | Less polished |

**New option from user context:** Use available SiteGround/Divi slot for dedicated API/samples site that includes Prompt Generator.

**Gaps Requiring Additional Research:**
- [x] ~~Existing deterministic prompt generators on GitHub~~ → Completed below
- [x] ~~How to extract/expose StoryCAD list data for web consumption~~ → Completed below
- [ ] Lead capture integration with Divi

---

### 2026-02-04 - Gap Research Completed

#### Gap 1: Deterministic Prompt Generator Patterns (via research-analyst agent)

**Key repositories studied:**

| Repository | Approach | Technology |
|------------|----------|------------|
| **[Tracery](https://github.com/galaxykate/tracery)** | Grammar-based symbol expansion | JS/Python |
| **[RiTa.js](https://github.com/dhowe/ritajs)** | Grammar + NLP toolkit | JS/Java |
| **[garykac/plotto](https://github.com/garykac/plotto)** | Hyperlinked conflict chains | HTML/JS |
| **[jcolag/AutoStory](https://github.com/jcolag/AutoStory)** | Hero's Journey + Dramatic Situations | Ruby |
| **[sharkham/prompt-generator](https://github.com/sharkham/prompt-generator)** | Simple random selection | React |

**Recommended implementation approaches (in order of complexity):**

1. **Simple Combinatorial** (Easiest)
   - Random selection from each category
   - Display as structured prompt card
   - Example: "Genre: Mystery | Theme: Betrayal | Protagonist: The Detective"

2. **Template-Based** (Medium)
   - Sentence templates with slot filling
   - "A [protagonist_archetype] must [dramatic_situation] while dealing with [theme]..."

3. **Grammar-Based / Tracery** (Most Flexible)
   - Symbol expansion with modifiers
   - Allows nested generation
   - More natural-sounding output

**UI patterns to consider:**
- Category toggles (enable/disable data sources)
- Regenerate individual elements (click to randomize one part)
- Lock elements (keep some, randomize others)
- History tracking

#### Gap 2: StoryCAD Data Extraction

**Data sources identified in StoryCADLib:**

| File | Format | Contents |
|------|--------|----------|
| `Lists.json` | `{key: string[]}` | 75 categories, ~2,600+ values |
| `Tools.json` | Structured objects | 347 stock scenes, 36 dramatic situations, 18 master plots |
| `Controls.json` | Nested categories | 8 conflict categories, 77 subcategories, 279 examples |

**Full inventory: [`data_inventory.md`](data_inventory.md)**

**Lists.json structure (simple key→array):**
```json
{
  "Tone": ["Accusing", "Aggravated", "Amiable", ...],
  "Genre": [...],
  "Theme": [...]
}
```

**Tools.json structure (richer data):**
```json
{
  "stockScenes": {
    "Western Stock Scenes": ["A guilty man is being lynched", ...],
    "Twists and Turns": ["Ally turns out to be on wrong side", ...]
  },
  "masterPlots": [
    { "name": "Adventure", "notes": "...", "scenes": [...] }
  ],
  "dramaticSituations": {
    "Supplication": { "roles": ["persecutor", "supplicant", "judge"], "notes": "..." }
  }
}
```

**Extraction options:**

The StoryCADApi already provides access to all three data sources:
- `ControlData` - ConflictTypes, RelationTypes
- `ListData` - All list controls (Tone, Genre, Theme, etc.)
- `ToolsData` - MasterPlots, DramaticSituations, StockScenes, etc.

| Option | Approach | When to Use |
|--------|----------|-------------|
| **API at build time** | Use StoryCADApi to generate static JSON | MVP and ongoing releases |
| **API at runtime** | Backend calls StoryCADApi | Phase 2 AI enhancement only |

**Recommendation:** Use API to generate static JSON for web consumption. The API provides properly structured access to ControlData, ListData, and ToolsData. Cloudflare Worker only needed later for AI enhancement, not for data access.

---

### 2026-01-26 - StoryCAD #1246 Research Reviewed
Reviewed `/mnt/d/dev/src/StoryCAD/devdocs/issue_1246_api_samples/`:
- **Prompt Generator is Sample #9** in the proposed API samples list
- Described as: "Generate prompts from Master Plots, Dramatic Situations, Stock Scenes"
- Fits into recommended v1 sample set
- API-Samples repo at `/mnt/d/dev/src/API-Samples/` (private, to be made public)
- StoryCADLib 4.0 supports headless mode for non-UI consumers

**Key data sources for prompts:**
- Master Plots (Quest, Revenge, Transformation, etc.)
- Dramatic Situations (36 classic situations from Polti)
- Stock Scenes (common scene patterns)
- ToolsData (conflict builder, trait spectrums, relationships)

**Web hosting strategy from #1246:**
- Options evaluated: storybuilder.org, user manual site, dedicated site, README-driven
- Example: pollydocs.org (dedicated site with clean separation)
- Possible approach: subdomain (prompts.storybuilder.org) or GitHub Pages, linked from main site

---

## Research Summary

### Core Design Decisions (from research)
1. **Structure, not prose** - Prompts generate story elements, not narrative text
2. **List-driven** - Use StoryCAD's existing lists (genres, themes, archetypes, traits)
3. **Human-in-the-loop** - Propose options, require confirmation, generate alternatives
4. **Incomplete by design** - Prompts invite authorship, not deliver finished stories

### Prompt Schema Components
- Conceptual Spine (genre, theme, tone)
- Character Seeds (protagonist role+flaw+desire, antagonist)
- Situational Hook (inciting condition, constraint)
- Structural Hints (conflict type, stakes level)
- World Signal (optional setting cues)

### Prompt Profiles
- Minimal (concept + situation)
- Character-first
- Problem-first
- World-first

---

## Open Questions
- ~~Web deployment method for WordPress/Divi site?~~ → See Deployment Options below
- Lead capture mechanism - email gate before use?
- ~~Use Cloudflare worker (#1136) for LLM calls?~~ → Only needed if AI-enhanced version chosen
- ~~Standalone web app vs embedded vs subdomain?~~ → See Deployment Options below
- Which prompt profile is highest priority for marketing?
- ~~How to extract StoryCAD list data for web consumption?~~ → Use StoryCADApi to generate static JSON

## Decisions Made
- Story Prompts are structural, not prose
- Leverage existing StoryCAD lists for field values
- Human confirmation required before instantiation
- Prompt Generator aligns with StoryCAD #1246 samples strategy (separate public repo)
- **Hybrid architecture preferred**: Deterministic base (list-driven) + optional AI enhancement
- **Deployment: Blazor WASM + GitHub Pages**
  - Host on public API-Samples repo (coordinate with #1246)
  - docfx for API documentation
  - Blazor WASM for interactive prompt generator
  - Free hosting, direct StoryCADLib access
  - Follows pollydocs.org model

## Deployment Options

**SELECTED: Option E - Blazor WASM on GitHub Pages (API-Samples repo)**

### Options Evaluated

| Option | Infrastructure | Cost | API Access | Status |
|--------|---------------|------|------------|--------|
| A: Divi site | SiteGround | Included | Static JSON only | ❌ C# access painful |
| B: GitHub Pages (static) | GitHub | Free | Static JSON only | ❌ No .NET |
| E: Blazor WASM | GitHub Pages | Free | Direct (.NET in browser) | ✅ **SELECTED** |
| F: ASP.NET + Razor | Azure/VPS | $0-20/mo | Direct (server-side) | ❌ Hosting cost |
| G: Azure Static Web Apps | Azure | Free tier | Direct (Functions) | ⚪ Viable alternative |

### Selected Approach: Blazor WASM + GitHub Pages

**Model:** [pollydocs.org](https://www.pollydocs.org/) (Polly .NET library docs)
- docfx for API documentation
- Blazor WASM for interactive prompt generator
- GitHub Pages hosting (free)
- GitHub Actions for CI/CD
- Custom domain optional (~$12/year)

**Why this works:**
- Free hosting (GitHub Pages)
- Direct StoryCADLib access (Blazor WASM runs .NET in browser)
- Consistent tech stack (.NET throughout)
- Follows proven Polly model
- API-Samples repo becomes a live demo (#1246 alignment)

**Prerequisites:**
- Make API-Samples repo public (already planned per #1246)
- Coordinate with #1246 for shared site structure

## Next Steps
- [x] ~~Decide deployment target~~ → Web-based for lead gen + API sample (both)
- [x] ~~Define minimum viable prompt schema~~ → Minimal profile documented in broad_story_prompt.md
- [x] ~~Research existing deterministic prompt generators~~ → Tracery, Plotto, combinatorial patterns
- [x] ~~Design data extraction~~ → Direct StoryCADLib access via Blazor WASM
- [x] ~~Decide deployment option~~ → Blazor WASM + GitHub Pages on API-Samples repo
- [x] ~~Create data inventory~~ → `data_inventory.md` (75 lists, 2.1B+ combinations)
- [x] ~~Map data to interaction patterns~~ → `interaction_design.md` (4 patterns, MVP defined)
- [ ] **Coordinate with #1246** to make API-Samples repo public
- [ ] Set up project structure (docfx + Blazor WASM)
- [ ] Configure GitHub Actions for build/deploy
- [ ] Finalize MVP field selection for each profile
- [ ] Design lead capture integration (custom Blazor form)
- [ ] Implement prompt generation logic
- [ ] Phase 2: AI enhancement via Collaborator

---

### 2026-02-04 - Console Prototype Created

**Prototype location:** `/mnt/d/dev/src/PromptGeneratorPrototype/`

Created a .NET 10 console app to demonstrate the prompt generation logic without web infrastructure. Loads actual StoryCAD data (Lists.json, Tools.json, Controls.json) and generates combinatorial prompts.

**Profiles implemented:**
1. Minimal (Genre, Theme, Tone, Dramatic Situation)
2. Character-First (Archetype, Role, Trait, Wound, Motive)
3. Problem-First (Conflict, Goal, Opposition, Outcome)
4. World-First (Genre, WorldType, Locale, Season, Tone, Stock Scene)
5. **Full Story Premise** (combines all into Protagonist vs Antagonist structure)

**Research: What Other Prompt Generators Produce**

Researched existing generators (Reddit WritingPrompts, Reedsy, Plot-Generator, etc.). Key finding:

> **Successful prompts synthesize elements into coherent narrative hooks** rather than just listing elements.

Five format types identified:
- Type A: Single-sentence hooks (most popular)
- Type B: Imperative instructions
- Type C: Element combinations (our initial approach)
- Type D: Scenario + questions
- Type E: Structured plot summaries

**Updated prototype to output both:**
- Raw elements (for users who want to pick and choose)
- Synthesized premise paragraph (narrative hook)

**StoryCAD Premise Workflow Integrated**

Found documentation describing Premise → Story Problem → Protagonist/Antagonist workflow:
- `/mnt/d/dev/src/StoryCAD/docs/Writing with StoryCAD/Story_Idea_Concept_and_Premise.md`
- `/mnt/d/dev/src/StoryCAD/docs/Story Elements/Resolution_Tab.md`

Key template from docs:
> "A character in a situation [genre, setting] wants something, which brings him into conflict with a second character [opposition]..."

Full Story Premise option now follows this pattern and includes "Next steps in StoryCAD" guidance.

**Known Issue: Contradictory Combinations**

Pure random generation can produce incoherent combinations:
- Theme: "Despair ends in optimism" + Outcome: "Villain is successful"
- Tone: "Encouraging" + dark outcomes

**Options to address (deferred to later phase):**
1. Categorize data with sentiment tags, apply constraints
2. Add rules (e.g., positive tone → exclude villain-wins outcomes)
3. AI polish (Phase 2) - LLM smooths contradictions
4. User control - lock preferred fields, regenerate incompatible ones

**For MVP:** Accept occasional odd combinations. Users can regenerate individual fields.

---

### RESUME POINT

**Console prototype complete.** Demonstrates:
- Data loading from StoryCAD JSON files
- Combinatorial generation across 5 profiles
- Premise synthesis following StoryCAD workflow
- Known limitation: contradictory combinations possible

**Next steps:**
1. Coordinate with #1246 to make API-Samples repo public
2. Port prototype logic to Blazor WASM
3. Design UI with lock/regenerate per field
4. Add lead capture form
