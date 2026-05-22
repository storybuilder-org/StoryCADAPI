# Issue #61: Fiction Prompt Generator - Research

## Overview

Research notes for building a fiction prompt generator in Collaborator.

---

## Business Context

### Purpose
- **Lead generation tool** - Recommended by Getting Attention (gettingattention.org), who manages StoryBuilder's Google Ad account
- Attract prospects to StoryCAD software through useful free tool
- Demonstrate StoryCAD/Collaborator AI capabilities

### Related Issues
- **StoryCAD #1246** - Update and release new samples and API guidance
  - Samples exist in private repo (`/mnt/d/dev/src/API-Samples/`) but never released
  - Need updates for UNO Platform (v4.0)
  - **Prompt Generator is explicitly listed as Sample #9** in the research:
    > "Generate prompts from Master Plots, Dramatic Situations, Stock Scenes"
  - Fits into recommended v1 sample set (3 core + 2 SK samples)
  - API-Samples repo to be made public with cross-linking to NuGet/docs

### Deployment Options

**Option A: Web-based (Lead Generation)**
- Standalone web tool accessible from storybuilder.org
- WordPress/Divi site integration challenges
- Could be: embedded iframe, subdomain, or linked external app
- Captures leads before directing to StoryCAD download

**Option B: API Sample Application**
- Demonstrates StoryCADLib API usage
- Part of samples/guidance for developers
- Desktop or web application

**Option C: Both**
- Web version for marketing/leads
- Sample app version for developers
- Shared core prompt generation logic

---

## Key Design Concept (from broad_story_prompt.md)

### What is a Story Prompt?

> **A Story Prompt is a *partial story state* composed of selected and generated elements that together imply a story worth developing.**

A Story Prompt:
- Is **structural**, not prose
- Is **incomplete by design**
- Can be expanded into Story Problems, Characters, Scenes, or full outlines
- Serves as a bridge between inspiration and structured development

### Design Rule

> **"Incomplete enough to invite authorship, but structured enough to imply story."**
>
> If it feels like a story summary, it is too far.
> If it feels like a random idea, it is not far enough.

---

## Story Prompt Schema (from research files)

### Core Components

| Component | Purpose | StoryCAD Mapping |
|-----------|---------|------------------|
| **Conceptual Spine** | High-level framing | Genre, Theme, Tone lists |
| **Character Seeds** | Lightweight starting points | Archetypes, Roles, Traits |
| **Situational Hook** | Condition demanding response | Problem fields |
| **Structural Implication** | Hints about story shape | Master Plots, Conflict types |
| **World Signal** | Light world cues | Setting, Tech/Magic level |

### Minimal Schema

```
StoryPrompt
├── PromptId
├── PromptName
├── IncludedElements
├── Concept
│   ├── Genre
│   ├── Theme
│   └── Tone
├── Characters
│   ├── ProtagonistSeed (role + flaw + desire)
│   └── AntagonistSeed (person/system/nature/self)
├── Situation
│   ├── IncitingCondition
│   └── CentralConstraint
├── StructuralHints
│   ├── ConflictType
│   └── StakesLevel
└── Notes (optional)
```

**Key insight**: No field requires prose. All fields should be concise and structured.

---

## Prompt Profiles (Development Idea)

Offer predefined prompt styles that determine which schema fields are active:

| Profile | Active Fields |
|---------|---------------|
| **Minimal** | Concept + Situation only |
| **Character-first** | Character seeds + flaw + desire emphasized |
| **Problem-first** | Conflict, stakes, antagonistic force emphasized |
| **World-first** | Setting, world rules, technology/magic emphasized |

---

## Story Element Prompt (Specific Use Case)

### Generate Story Problem from Theme + Genre

**Human Inputs (UI):**
- Genre: (from StoryCAD list)
- Theme: (from StoryCAD list)
- ProtagonistFlaw: (from StoryCAD traits)
- Tone (optional): (from list)

**Output Schema:**
- ProtagonistGoal
- AntagonistGoal
- CentralConflict
- Stakes
- PossibleOutcomeVariants (2-3 options)

**Design Principle:** Generates structure, not prose. Human remains in control.

---

## Integration with StoryCAD

### Option 1: New Prompt Node Type
Add a `Prompt` node to StoryCAD with fields:
- Genre
- Focus (Character, Problem, Scene)
- RawPromptText

### Option 2: API Method
```csharp
CreatePromptNode(OutlineElement element, string promptText)
```
Attaches generated prompt directly to any node.

### Option 3: UI "Generate Prompt..." Button
1. Serialize current outline node
2. Send to LLM service
3. Insert prompt as child node or populate fields

---

## Human-in-the-Loop Control

Collaborator should:
- **Propose options**, not finalize choices
- **Generate alternatives**, not answers
- **Require explicit human confirmation** before instantiation

---

## List-Driven Generation

Favor selection from existing StoryCAD lists:
- Genres
- Themes
- Archetypes
- Conflict types
- Traits

AI fills gaps **only after** list choices are made.

---

## StoryCAD Data Sources for Prompt Generation

**Full inventory: [`data_inventory.md`](data_inventory.md)** - Complete catalog with counts.

From issue #1246 research - StoryCAD has rich existing data for prompt generation:

### Master Plots
Pre-defined plot structures (e.g., Quest, Revenge, Transformation, Discovery)
- Can generate prompts based on plot type requirements
- Each master plot has structural expectations

### Dramatic Situations
36 classic dramatic situations (from Polti's analysis)
- Supplication, Deliverance, Crime Pursued by Vengeance, etc.
- Rich source of conflict and character dynamics

### Stock Scenes
Common scene types with established patterns
- Chase scenes, confrontations, revelations, etc.
- Can generate prompts for specific scene needs

### Tools Data (ToolsData in StoryCADLib)
- Conflict Builder data
- Character trait spectrums
- Relationship types

### API Access
```csharp
// Via StoryCADApi
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
// Access to ControlData, ListData, ToolsData resources
```

These data sources should be primary inputs for prompt generation, with AI filling gaps and creating combinations.

---

## Example Generated Prompt

```
Concept:
  Genre: Near-future Science Fiction
  Theme: Control vs autonomy
  Tone: Somber

Characters:
  ProtagonistSeed: Reluctant caretaker with conflict-avoidant trait
  AntagonistSeed: Bureaucratic system optimized for efficiency

Situation:
  IncitingCondition: A routine exception exposes a hidden rule
  CentralConstraint: Acting requires violating a signed agreement

StructuralHints:
  ConflictType: Individual vs system
  StakesLevel: Societal
```

This prompt is immediately usable:
- Convert to a Story Problem
- Expand characters
- Generate candidate scenes
- Or remain as a saved "starter state"

---

## Infrastructure Questions

- How to host web app from WordPress/Divi site?
- Authentication/rate limiting for public tool?
- Lead capture mechanism (email before use?)
- Connection to existing Collaborator LLM infrastructure (#1136 Cloudflare worker?)

---

## Web Hosting Strategy (from #1246 research)

Issue #1246 research evaluated web hosting options for API-related tools:

| Option | Example | Pros | Cons |
|--------|---------|------|------|
| Add to storybuilder.org | - | Single brand, no new infra | Different audience |
| Add to user manual | storybuilder-org.github.io/StoryCAD/ | Already exists, Jekyll | Mixes audiences |
| **Dedicated site** | pollydocs.org | Clean separation, polished | More to maintain |
| README-driven in repo | GitHub markdown | Simplest, co-located | Less polished |

### Examples to Study
- **Polly**: https://www.pollydocs.org/ (dedicated site, clean separation)
- **Semantic Kernel**: Microsoft Learn (integrated into larger docs)
- **UNO Platform**: platform.uno/docs (dedicated)

### Considerations for Prompt Generator
- **Educational mission** - accessibility for learners
- **Small team** - maintenance burden matters
- **Lead generation goal** - needs to be discoverable, polished
- **WordPress/Divi limitation** - storybuilder.org may not easily host interactive apps

### Possible Approach
- Host prompt generator on subdomain (e.g., `prompts.storybuilder.org`) or GitHub Pages
- Link prominently from main storybuilder.org site
- Keep WordPress for marketing content, separate hosting for interactive tool
- Similar to pollydocs.org pattern: dedicated tool site with cross-links to main brand

---

## Collaborator Workflow Patterns (from workflow_development_guide.md)

If implemented as a Collaborator workflow:

### Workflow Structure
- Registration in `WorkflowRegistry.cs` (defines metadata and I/O contract)
- Prompt in `Plugins/WorkflowPlans/{Label}/skprompt.txt`
- Config in `Plugins/WorkflowPlans/{Label}/config.json`
- Sources in `Plugins/WorkflowPlans/{Label}/sources.md`

### Key Patterns to Follow
- **One Property = One Idea** - Each output field holds one concept, not compound content
- **Terseness as Layered Expansion** - Concise = core, Balanced = core + context, Detailed = full elaboration
- **Content Preservation** - Strict/Balanced/Flexible controls what AI can modify
- **JSON Output** - Property names must exactly match StoryCAD API names (PascalCase, flat structure)

### User Settings Integration
Workflows can use `{{$UserSettings}}` for:
- Response Terseness (Concise, Balanced, Detailed)
- Content Preservation (Strict, Balanced, Flexible)
- Genre Preferences
- Story Forms preferences

### Prompt Variable Pattern
`{{$ElementLabel_PropertyName}}` e.g., `{{$Overview_Genre}}`, `{{$Problem_ProtGoal}}`

For dynamic examples from StoryCAD lists: `{{$PropertyName_examples}}`

---

## Technical Notes

### C# LLM Integration (from samples)

```csharp
var client = new OpenAIClient(apiKey);
var prompt = await client.Chat.GetChatCompletionsAsync(new ChatCompletionsOptions
{
    Messages =
    {
        new ChatMessage("system", "You are an assistant that creates short creative writing prompts."),
        new ChatMessage("user", context)
    },
    Model = "gpt-4o-mini"
});
return prompt.Value.Choices.Message.Content;
```

Collaborator already has Semantic Kernel integration - should use that pattern.

---

## Next Steps

1. Decide deployment target (web, desktop sample, or both)
2. Define minimum viable prompt schema
3. Create Collaborator workflow or standalone tool
4. Design lead capture mechanism if web-based
5. Test with real users
