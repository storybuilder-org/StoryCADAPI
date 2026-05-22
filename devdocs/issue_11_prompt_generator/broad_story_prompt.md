# Broad Story Prompt Definition (StoryCAD)

This document defines a **broad, generator-style story prompt** suitable for StoryCAD and Collaborator.
It aligns with what existing prompt generators produce, while leveraging StoryCAD’s structured data,
lists, and tools.

The goal is to create **starting conditions for a story**, not prose and not a full outline.

---

## 1. Definition: Story Prompt (StoryCAD)

A **Story Prompt** is a *partial story state* composed of selected and generated elements that together
imply a story worth developing.

A Story Prompt:
- Is **structural**, not prose
- Is **incomplete by design**
- Can be expanded into a Story Problem, Characters, Scenes, or full outlines
- Serves as a bridge between inspiration and structured development

---

## 2. Core Components

A prompt may include any subset of the following components.

### 2.1 Conceptual Spine
High-level framing, usually list-driven.

- Genre
- Theme
- Core Dramatic Question (implicit or explicit)
- Tonal Direction

These should map directly to StoryCAD genre, theme, and tone lists.

---

### 2.2 Character Seeds
Lightweight starting points, not full character definitions.

- Protagonist role or archetype
- Primary flaw or limitation
- Antagonistic force (person, system, nature, self)
- Core value conflict between forces

StoryCAD already supports:
- Archetypes
- Roles
- Traits
These should be reused rather than reinvented.

---

### 2.3 Situational Hook
The immediate condition that demands response.

- Inciting condition
- Constraint or pressure
- Moral, emotional, or practical dilemma

This replaces traditional one-line prompts with structured fields.

---

### 2.4 Structural Implication
Hints about story shape, not commitments.

- Conflict type (e.g., individual vs system)
- Scale (personal, community, societal)
- Expected trajectory (rise, fall, spiral, restoration)

These can connect to:
- Master Plots
- Problem templates
- Beat or act frameworks

---

### 2.5 Optional World Signal
Light world cues when relevant.

- Setting type
- Technology or magic level
- Abnormal rule, law, or condition

All should be selectable from lists when possible.

---

## 3. Revised Minimal Schema

```
StoryPrompt
- PromptId
- PromptName
- IncludedElements
- Concept
  - Genre
  - Theme
  - Tone
- Characters
  - ProtagonistSeed
  - AntagonistSeed
- Situation
  - IncitingCondition
  - CentralConstraint
- StructuralHints
  - ConflictType
  - StakesLevel
- Notes (optional, human-editable)
```

No field requires prose. All fields should be concise and structured.

---

## 4. Example Generated Prompt

```
Concept:
- Genre: Near-future Science Fiction
- Theme: Control vs autonomy
- Tone: Somber

Characters:
- ProtagonistSeed: Reluctant caretaker with conflict-avoidant trait
- AntagonistSeed: Bureaucratic system optimized for efficiency

Situation:
- IncitingCondition: A routine exception exposes a hidden rule
- CentralConstraint: Acting requires violating a signed agreement

StructuralHints:
- ConflictType: Individual vs system
- StakesLevel: Societal
```

This prompt is immediately usable:
- Convert to a Story Problem
- Expand characters
- Generate candidate scenes
- Or remain as a saved “starter state”

---

## 5. Development Ideas

### 5.1 Prompt Profiles
Offer predefined prompt styles:
- Minimal (concept + situation)
- Character-first
- Problem-first
- World-first

Profiles determine which schema fields are active.

---

### 5.2 List-Driven Generation
Favor selection from existing StoryCAD lists:
- Genres
- Themes
- Archetypes
- Conflict types

AI fills gaps only after list choices are made.

---

### 5.3 Human-in-the-Loop Control
Collaborator should:
- Propose options, not finalize choices
- Generate alternatives, not answers
- Require explicit human confirmation before instantiation

---

### 5.4 Prompt as Asset
Treat prompts as:
- Versioned
- User-editable
- Shareable

Advanced users should be able to refine prompt logic over time.

---

## 6. Design Rule

A Story Prompt must be:

> **Incomplete enough to invite authorship, but structured enough to imply story.**

If it feels like a story summary, it is too far.
If it feels like a random idea, it is not far enough.
