# Resource Data and Writing Tools

StoryCADLib ships with built-in reference data designed to help fiction writers develop their stories. The API exposes this data through discovery methods that follow a consistent pattern: call a listing method first, then drill into specifics.

## Resource Categories

| Resource | API Entry Point | Steps | Purpose |
|----------|----------------|-------|---------|
| [Example Lists](#example-lists) | `GetExamples` | 1 | Valid values for element properties |
| [Conflict Builder](#conflict-builder) | `GetConflictCategories` | 4 | Structured conflict development |
| [Key Questions](#key-questions) | `GetKeyQuestionElements` | 2 | Guided element development prompts |
| [Master Plots](#master-plots) | `GetMasterPlotNames` | 3 | Story structure templates (Tobias) |
| [Stock Scenes](#stock-scenes) | `GetStockSceneCategories` | 2 | Scene inspiration by genre |
| [Beat Sheets](#beat-sheets) | `GetBeatSheetNames` | 3+ | Story structure frameworks |

## Example Lists

The simplest resource — a single call returns valid values for a property.

```csharp
var tones = api.GetExamples("Tone").Payload;
// ["Humorous", "Serious", "Dark", "Light", ...]
```

**Available property names**: Tone, Role, Archetype, Sex, Age, Build, Complexion, EyeColor, HairColor, HairStyle, Demeanor, Enneagram, Intelligence, Values, Abnormality, Focus, Adventuresome, Aggression, Confidence, Conscientiousness, Creativity, Dominance, Enthusiasm, Assurance, Sensitivity, Shrewdness, Sociability, Stability, ScenePurpose, SceneType, SceneValueExchange, SettingCountry, Locale, Season

These values are suggestions, not constraints. You can pass any string to `UpdateElementProperty` — the examples help users and AI agents discover the intended vocabulary.

See [Element Types and Properties](element-types.md) for which properties accept which example lists.

## Conflict Builder

A 4-step workflow for developing story conflicts. Each step narrows the scope.

| Step | Method | Input | Output |
|------|--------|-------|--------|
| 1 | `GetConflictCategories()` | — | Category names |
| 2 | `GetConflictSubcategories(category)` | Category name | Subcategory names |
| 3 | `GetConflictExamples(category, subcategory)` | Category + subcategory | Conflict descriptions |
| 4 | `ApplyConflictToProtagonist(problemGuid, text)` | Problem GUID + text | Applied to Problem |
| 4 | `ApplyConflictToAntagonist(problemGuid, text)` | Problem GUID + text | Applied to Problem |

**Categories include**: Relationship, Situational, Inner Conflict, Paranormal, Criminal activities, Mystery and suspense, Social drama, Romantic

Step 4 has two variants — one for the protagonist's conflict, one for the antagonist's. You can also use custom text instead of a value from step 3.

See [Resource Workflows](../operations/resource-workflows.md) for a complete code example.

## Key Questions

A 2-step workflow that returns development prompts organized by topic.

| Step | Method | Input | Output |
|------|--------|-------|--------|
| 1 | `GetKeyQuestionElements()` | — | Element type names |
| 2 | `GetKeyQuestions(elementType)` | Element type name | (Topic, Question) tuples |

**Common element types**: Character, Problem, Scene, Setting, Overview

Each question is a prompt to help the author think about a specific aspect of the element. For example, Character key questions might include topics like "Motivation", "Backstory", and "Relationships" with specific questions for each.

See [Resource Workflows](../operations/resource-workflows.md) for a complete code example.

## Master Plots

A 3-step workflow based on Tobias's *20 Master Plots*. Each plot pattern includes descriptive notes and a scene breakdown.

| Step | Method | Input | Output |
|------|--------|-------|--------|
| 1 | `GetMasterPlotNames()` | — | Plot pattern names |
| 2 | `GetMasterPlotNotes(plotName)` | Plot name | Descriptive text |
| 3 | `GetMasterPlotScenes(plotName)` | Plot name | (SceneTitle, Notes) tuples |

**Available plots**: Quest, Adventure, Pursuit, Rescue, Escape, Revenge, The Riddle, Rivalry, Underdog, Temptation, Metamorphosis, Transformation, Maturation, Love, Forbidden Love, Sacrifice, Discovery, Wretched Excess, Ascension, Descension

The scene breakdown from step 3 can be used to create Scene elements with `AddElement`, giving the story a structure based on a classic plot pattern.

See [Resource Workflows](../operations/resource-workflows.md) for a complete code example.

## Stock Scenes

A 2-step workflow for scene inspiration organized by genre or purpose.

| Step | Method | Input | Output |
|------|--------|-------|--------|
| 1 | `GetStockSceneCategories()` | — | Category names |
| 2 | `GetStockScenes(category)` | Category name | Scene descriptions |

Stock scene descriptions can be used as the `Description` property when creating Scene elements, or as inspiration for more detailed scene development.

See [Resource Workflows](../operations/resource-workflows.md) for a complete code example.

## Beat Sheets

Beat sheets are story structure frameworks (Three Act Structure, Hero's Journey, Save the Cat, etc.) that define a sequence of narrative beats. They have the most complex workflow because they integrate with the Problem element's structure.

| Step | Method | Input | Output |
|------|--------|-------|--------|
| 1 | `GetBeatSheetNames()` | — | Template names |
| 2 | `GetBeatSheet(name)` | Template name | (Description, Beats) |
| 3 | `ApplyBeatSheetToProblem(problemGuid, name)` | Problem GUID + name | Beats applied |
| 4+ | `AssignElementToBeat`, `CreateBeat`, etc. | Various | Customize structure |

Beat sheets are covered in full detail in [Beat Sheet Operations](../operations/beat-sheets.md).

## Access Pattern Summary

All resource APIs follow the same pattern:

1. **Discover** — call a listing method to see what's available
2. **Inspect** — call a detail method to understand a specific item
3. **Apply** — use the data to create or update story elements

No resource method modifies the story model until the final "apply" step. The discovery and inspection steps are read-only and safe to call at any time.
