---
layout: default
title: "Element Types and Properties"
parent: "Concepts"
nav_order: 2
---

# Element Types and Properties

Every element in a StoryCAD outline is a `StoryElement` subclass identified by its `StoryItemType`. This page documents all 11 types, their properties, and the rules for creating and linking them.

## Base Properties (All Elements)

Every `StoryElement` has these properties (inherited from the base class):

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Uuid` | `Guid` | `GUID` | Unique identifier |
| `Name` | `string` | `Name` | Display name |
| `Description` | `string` | `ElementDescription` | Main text content |
| `ElementType` | `StoryItemType` | `Type` | Discriminator (read-only via API) |

Use the `Name` and `Description` property names with `UpdateElementProperty`.

## StoryItemType Enum

<div class="code-tabs" markdown="1">

```csharp
public enum StoryItemType
{
    StoryOverview,  // Singleton: story metadata
    Problem,        // Story problem / conflict
    Character,      // Character profile
    Setting,        // Location / environment
    Scene,          // A scene in the story
    Folder,         // Explorer View organizer
    Section,        // Narrator View organizer (chapter, act)
    Web,            // Web bookmark
    Notes,          // Notes element (uses base properties only)
    TrashCan,       // Singleton: deleted elements container
    StoryWorld,     // Singleton (optional): worldbuilding
    Unknown         // Sentinel: not a usable element type
}
```

```python
# The StoryItemType enum is exposed as sc.item_type (C#-type concept):
sc.item_type.StoryOverview  # Singleton: story metadata
sc.item_type.Problem        # Story problem / conflict
sc.item_type.Character      # Character profile
sc.item_type.Setting        # Location / environment
sc.item_type.Scene          # A scene in the story
sc.item_type.Folder         # Explorer View organizer
sc.item_type.Section        # Narrator View organizer (chapter, act)
sc.item_type.Web            # Web bookmark
sc.item_type.Notes          # Notes element (uses base properties only)
sc.item_type.TrashCan       # Singleton: deleted elements container
sc.item_type.StoryWorld     # Singleton (optional): worldbuilding
```

</div>

**Creatable via `AddElement`**: Problem, Character, Setting, Scene, Folder, Section, Web, Notes

**Not creatable via `AddElement`**: StoryOverview, TrashCan (auto-created), StoryWorld (auto-created)

## Element Type Reference

### StoryOverview (`OverviewModel`)

The story's root element. One per outline, always the first node in ExplorerView.

| Property | Type | Value Source | Description |
|----------|------|-------------|-------------|
| `DateCreated` | `string` | Free text | Creation date |
| `Author` | `string` | Free text | Author name |
| `DateModified` | `string` | Free text | Last modified date |
| `Concept` | `string` | Free text | Story concept / idea |
| `StoryProblem` | `Guid` | GUID ref → Problem | The main story problem |
| `Premise` | `string` | Free text | Story premise (synced with StoryProblem) |
| `StoryType` | `string` | Free text | Type of story |
| `StoryGenre` | `string` | Free text | Genre |
| `Viewpoint` | `string` | Free text | Narrative viewpoint |
| `ViewpointCharacter` | `Guid` | GUID ref → Character | First-person POV character |
| `Voice` | `string` | Free text | Narrative voice |
| `LiteraryDevice` | `string` | Free text | Literary devices used |
| `Tense` | `string` | Free text | Past/present tense |
| `Style` | `string` | Free text | Writing style |
| `Tone` | `string` | `GetExamples("Tone")` | Story tone |
| `StructureNotes` | `string` | Free text | Notes on structure |
| `Notes` | `string` | Free text | General notes |

### Problem (`ProblemModel`)

A story conflict with protagonist, antagonist, and optional beat sheet structure.

| Property | Type | Value Source | Description |
|----------|------|-------------|-------------|
| `ProblemType` | `string` | Free text | Type of problem |
| `ConflictType` | `string` | Free text | Conflict classification |
| `ProblemCategory` | `string` | Free text | Category |
| `Subject` | `string` | Free text | What the problem is about |
| `ProblemSource` | `string` | Free text | Source of the problem |
| `Protagonist` | `Guid` | GUID ref → Character | The protagonist |
| `ProtGoal` | `string` | Free text | Protagonist's goal |
| `ProtMotive` | `string` | Free text | Protagonist's motivation |
| `ProtConflict` | `string` | Free text / Conflict Builder | Protagonist's conflict |
| `Antagonist` | `Guid` | GUID ref → Character | The antagonist |
| `AntagGoal` | `string` | Free text | Antagonist's goal |
| `AntagMotive` | `string` | Free text | Antagonist's motivation |
| `AntagConflict` | `string` | Free text / Conflict Builder | Antagonist's conflict |
| `Outcome` | `string` | Free text | How the problem resolves |
| `Method` | `string` | Free text | Method of resolution |
| `Theme` | `string` | Free text | Thematic statement |
| `Premise` | `string` | Free text | Problem premise |
| `Notes` | `string` | Free text | General notes |
| `StructureTitle` | `string` | Beat sheet name | Applied beat sheet template |
| `StructureDescription` | `string` | Beat sheet description | Description of structure |
| `StructureBeats` | Collection | Beat Sheet API | Beat definitions (managed via beat sheet methods) |

> **Note**: `StructureBeats` is managed through the [Beat Sheet API](../operations/beat-sheets.md), not through `UpdateElementProperty`.

### Character (`CharacterModel`)

A character profile with physical, social, psychological, and trait data.

**Role Data**:

| Property | Type | Value Source |
|----------|------|-------------|
| `Role` | `string` | `GetExamples("Role")` |
| `StoryRole` | `string` | Free text |
| `Archetype` | `string` | `GetExamples("Archetype")` |

**Physical Data**:

| Property | Type | Value Source |
|----------|------|-------------|
| `Age` | `string` | `GetExamples("Age")` |
| `Sex` | `string` | `GetExamples("Sex")` |
| `Eyes` | `string` | `GetExamples("EyeColor")` |
| `Hair` | `string` | `GetExamples("HairColor")` |
| `Weight` | `string` | Free text |
| `CharHeight` | `string` | Free text |
| `Build` | `string` | `GetExamples("Build")` |
| `Complexion` | `string` | `GetExamples("Complexion")` |
| `Race` | `string` | Free text |
| `Nationality` | `string` | Free text |
| `Health` | `string` | Free text |
| `PhysNotes` | `string` | Free text |
| `Appearance` | `string` | Free text |

**Social Data**:

| Property | Type | Value Source |
|----------|------|-------------|
| `Economic` | `string` | Free text |
| `Education` | `string` | Free text |
| `Ethnic` | `string` | Free text |
| `Religion` | `string` | Free text |

**Psychological Data**:

| Property | Type | Value Source |
|----------|------|-------------|
| `Enneagram` | `string` | `GetExamples("Enneagram")` |
| `Intelligence` | `string` | `GetExamples("Intelligence")` |
| `Values` | `string` | `GetExamples("Values")` |
| `Abnormality` | `string` | `GetExamples("Abnormality")` |
| `Focus` | `string` | `GetExamples("Focus")` |
| `PsychNotes` | `string` | Free text |

**Personality Traits** (all use `GetExamples` with matching property name):

| Property | Type |
|----------|------|
| `Adventureousness` | `string` |
| `Aggression` | `string` |
| `Confidence` | `string` |
| `Conscientiousness` | `string` |
| `Creativity` | `string` |
| `Dominance` | `string` |
| `Enthusiasm` | `string` |
| `Assurance` | `string` |
| `Sensitivity` | `string` |
| `Shrewdness` | `string` |
| `Sociability` | `string` |
| `Stability` | `string` |

**Other**:

| Property | Type | Description |
|----------|------|-------------|
| `TraitList` | `List<string>` | Character traits (list) |
| `Flaw` | `string` | Character flaw |
| `BackStory` | `string` | Character backstory |
| `Notes` | `string` | General notes |
| `RelationshipList` | `List<RelationshipModel>` | Managed via `AddRelationship` |

> **Note**: Use `AddRelationship` and `AddCastMember` instead of `UpdateElementProperty` for relationship and cast member fields.

### Scene (`SceneModel`)

A scene with conflict structure, cast, setting, and development data.

**Core Data**:

| Property | Type | Value Source | Description |
|----------|------|-------------|-------------|
| `ViewpointCharacter` | `Guid` | GUID ref → Character | POV character |
| `Date` | `string` | Free text | When the scene occurs |
| `Time` | `string` | Free text | Time of day |
| `Setting` | `Guid` | GUID ref → Setting | Where the scene occurs |
| `SceneType` | `string` | `GetExamples("SceneType")` | Type of scene |
| `CastMembers` | `List<Guid>` | GUID refs → Characters | Use `AddCastMember` |

**Conflict Data**:

| Property | Type | Value Source |
|----------|------|-------------|
| `Protagonist` | `Guid` | GUID ref → Character |
| `ProtagEmotion` | `string` | Free text |
| `ProtagGoal` | `string` | Free text |
| `Antagonist` | `Guid` | GUID ref → Character |
| `AntagEmotion` | `string` | Free text |
| `AntagGoal` | `string` | Free text |
| `Opposition` | `string` | Free text |
| `Outcome` | `string` | Free text |

**Development Data** (Story Genius method):

| Property | Type | Value Source |
|----------|------|-------------|
| `ScenePurpose` | `List<string>` | `GetExamples("ScenePurpose")` |
| `ValueExchange` | `string` | `GetExamples("SceneValueExchange")` |
| `Events` | `string` | Free text |
| `Consequences` | `string` | Free text |
| `Significance` | `string` | Free text |
| `Realization` | `string` | Free text |
| `Emotion` | `string` | Free text |
| `NewGoal` | `string` | Free text |
| `Review` | `string` | Free text |
| `Notes` | `string` | Free text |

### Setting (`SettingModel`)

A location with sensory and environmental details.

| Property | Type | Value Source |
|----------|------|-------------|
| `Locale` | `string` | `GetExamples("Locale")` |
| `Season` | `string` | `GetExamples("Season")` |
| `Period` | `string` | Free text |
| `Lighting` | `string` | Free text |
| `Weather` | `string` | Free text |
| `Temperature` | `string` | Free text |
| `Props` | `string` | Free text |
| `Sights` | `string` | Free text |
| `Sounds` | `string` | Free text |
| `Touch` | `string` | Free text |
| `SmellTaste` | `string` | Free text |
| `Notes` | `string` | Free text |

### StoryWorld (`StoryWorldModel`)

Optional singleton for worldbuilding. Access via `GetStoryWorld()`.

**World Type Classification**:

| Property | Type | Description |
|----------|------|-------------|
| `WorldType` | `string` | High-level classification (8 types) |
| `Ontology` | `string` | What exists (Mundane, Supernatural, etc.) |
| `WorldRelation` | `string` | Relation to our reality |
| `RuleTransparency` | `string` | How rules behave |
| `ScaleOfDifference` | `string` | How different from reality |
| `AgencySource` | `string` | What drives change |
| `ToneLogic` | `string` | Emotional/logical feel |

**World Lists** (each is a list of structured entries):

| Property | Entry Type |
|----------|-----------|
| `PhysicalWorlds` | `PhysicalWorldEntry` |
| `Species` | `SpeciesEntry` |
| `Cultures` | `CultureEntry` |
| `Governments` | `GovernmentEntry` |
| `Religions` | `ReligionEntry` |

**History**: `FoundingEvents`, `MajorConflicts`, `Eras`, `TechnologicalShifts`, `LostKnowledge`

**Economy**: `EconomicSystem`, `Currency`, `TradeRoutes`, `Professions`, `WealthDistribution`

**Magic/Technology**: `SystemType`, `Source`, `Rules`, `Limitations`, `Cost`, `Practitioners`, `SocialImpact`

### Folder (`FolderModel`)

A container for organizing elements in the Explorer View. Uses only the base `Name` and `Description` properties.

### Section (`FolderModel`)

A container for organizing the Narrator View (chapters, acts, parts). Same model class as Folder but with `StoryItemType.Section`.

### Web (`WebModel`)

A web bookmark.

| Property | Type | Description |
|----------|------|-------------|
| `Timestamp` | `DateTime` | When the bookmark was created |
| `URL` | `Uri` | The bookmarked URL |

### Notes

Uses the base `StoryElement` class directly. Only `Name` and `Description` properties.

### TrashCan (`TrashCanModel`)

Singleton container for soft-deleted elements. No additional properties beyond the base class. Always named "Deleted Story Elements".

## Containment Rules

| Parent | Allowed Children |
|--------|-----------------|
| StoryOverview | Problem, Character, Setting, Scene, Folder, Web, Notes, StoryWorld |
| Folder | Problem, Character, Setting, Scene, Folder, Web, Notes |
| Section | Section, Scene |
| TrashCan | Any deleted element |

**Key constraints**:
- Don't add Folders to the Narrator View; use Sections instead
- Don't add Sections to the Explorer View; use Folders instead
- Don't add elements directly to the TrashCan; use `DeleteElement`
- Don't add elements to the Narrator View unless explicitly needed; default to the Overview

## Property Value Types

Properties fall into three categories:

### Plain Strings
Free text, pass any string value:
<div class="code-tabs" markdown="1">

```csharp
api.UpdateElementProperty(guid, "ProtGoal", "Find the treasure");
```

```python
sc.update_element_property(problem, "ProtGoal", "Find the treasure")
```

</div>

### GUID References
Link to another element, pass the target element's GUID as a string:
<div class="code-tabs" markdown="1">

```csharp
api.UpdateElementProperty(problemGuid, "Protagonist", characterGuid.ToString());
```

```python
sc.update_element_property(problem, "Protagonist", str(character))
```

</div>
The API auto-converts the string to a `Guid`. Use `Guid.Empty` (all zeros) to clear a reference.

### Enum-Sourced Values
Use `GetExamples` to discover valid values:
<div class="code-tabs" markdown="1">

```csharp
var roles = api.GetExamples("Role").Payload;
// Returns: ["Protagonist", "Antagonist", "Confidant", ...]
api.UpdateElementProperty(charGuid, "Role", "Protagonist");
```

```python
roles = sc.get_examples("Role")
# Returns: ["Protagonist", "Antagonist", "Confidant", ...]
sc.update_element_property(character, "Role", "Protagonist")
```

</div>
You can also pass free text for these properties. The examples are suggestions, not constraints.
