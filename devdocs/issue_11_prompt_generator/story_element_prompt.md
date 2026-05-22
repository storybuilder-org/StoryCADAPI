# Story Element Prompt Schema

This document defines a **minimal, StoryCAD-native prompt schema** for use with Collaborator,
followed by a **first concrete use case**. The focus is on generating *story structure*, not prose.

---

## 1. Minimal Prompt Schema (Structural, Not Prose)

The schema is intentionally small and rigid to ensure clean mapping to StoryCAD elements.

### Prompt Definition

```
PromptType
- Id
- Name
- TargetElementType   // StoryProblem, Character, Scene, Setting
- RequiredInputs      // human-supplied constraints
- OptionalInputs
- OutputSchema        // strict fields expected back
- SystemInstruction  // invariant rules
```

---

### Example: Story Problem Prompt Schema

```
PromptType: StoryProblemGenerator

RequiredInputs:
- Genre
- Theme
- ProtagonistFlaw

OptionalInputs:
- SettingConstraint
- Tone

OutputSchema:
- ProtagonistGoal
- AntagonistGoal
- CentralConflict
- Stakes
- PossibleOutcomeVariants (array, 2–3 items)

SystemInstruction:
- Output must be concise, structural, and non-prose.
- No scene writing.
- Each field must be 1–2 sentences max.
```

This schema maps directly to the existing **Story Problem** model in StoryCAD.

---

## 2. First Concrete Use Case

### Use Case: Generate a Story Problem from Theme + Genre

#### Human Inputs (UI)

```
Genre: Science Fiction
Theme: Identity vs conformity
ProtagonistFlaw: Avoids confrontation
Tone (optional): Somber
```

---

### Collaborator Prompt (Internal)

```
You are assisting a fiction writer by generating a STORY PROBLEM,
not a story.

Follow the OutputSchema exactly.
Do not write prose.
Provide structural story elements only.
```

---

### Expected AI Output (Example)

```
ProtagonistGoal:
Expose the truth about an identity-suppressing system before it becomes permanent.

AntagonistGoal:
Maintain social stability by enforcing uniform identities, regardless of individual cost.

CentralConflict:
The protagonist must act publicly to reveal the truth but is psychologically inclined to remain silent and compliant.

Stakes:
If the protagonist fails, personal identity becomes irreversibly erased across society; if they succeed, social collapse may follow.

PossibleOutcomeVariants:
- The system is dismantled but at great social upheaval.
- The protagonist preserves identity for a minority while the system persists.
- The protagonist sacrifices their own identity to expose the truth.
```

---

## Design Principles Recap

- Generates **structure, not prose**
- Keeps the **human in control**
- Maps cleanly to **StoryCAD story elements**
- Suitable for versioned, user-editable prompt assets
