---
layout: default
title: Models
parent: API Reference
nav_order: 3
---

# Models

Reference for the consumer-facing types in the `StoryCADLib.Models` namespace.

{: .fs-6 .fw-300 }

## CharacterModel

*Class*: `StoryCADLib.Models.CharacterModel`

```csharp
public class CharacterModel : StoryElement
```

### Constructors

#### CharacterModel(StoryModel, StoryNodeItem)

```csharp
public CharacterModel(StoryModel model, StoryNodeItem node)
```

**Parameters**

- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

#### CharacterModel(string, StoryModel, StoryNodeItem)

```csharp
public CharacterModel(string name, StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### CharacterModel()

```csharp
public CharacterModel()
```

### Properties

#### Role

```csharp
[JsonInclude]
[JsonPropertyName("Role")]
public string Role { get; set; }
```

**Type** `string`

#### StoryRole

```csharp
[JsonInclude]
[JsonPropertyName("StoryRole")]
public string StoryRole { get; set; }
```

**Type** `string`

#### Archetype

```csharp
[JsonInclude]
[JsonPropertyName("Archetype")]
public string Archetype { get; set; }
```

**Type** `string`

#### Age

```csharp
[JsonInclude]
[JsonPropertyName("Age")]
public string Age { get; set; }
```

**Type** `string`

#### Sex

```csharp
[JsonInclude]
[JsonPropertyName("Sex")]
public string Sex { get; set; }
```

**Type** `string`

#### Eyes

```csharp
[JsonInclude]
[JsonPropertyName("Eyes")]
public string Eyes { get; set; }
```

**Type** `string`

#### Hair

```csharp
[JsonInclude]
[JsonPropertyName("Hair")]
public string Hair { get; set; }
```

**Type** `string`

#### Weight

```csharp
[JsonInclude]
[JsonPropertyName("Weight")]
public string Weight { get; set; }
```

**Type** `string`

#### CharHeight

```csharp
[JsonInclude]
[JsonPropertyName("CharHeight")]
public string CharHeight { get; set; }
```

**Type** `string`

#### Build

```csharp
[JsonInclude]
[JsonPropertyName("Build")]
public string Build { get; set; }
```

**Type** `string`

#### Complexion

```csharp
[JsonInclude]
[JsonPropertyName("Complexion")]
public string Complexion { get; set; }
```

**Type** `string`

#### Race

```csharp
[JsonInclude]
[JsonPropertyName("Race")]
public string Race { get; set; }
```

**Type** `string`

#### Nationality

```csharp
[JsonInclude]
[JsonPropertyName("Nationality")]
public string Nationality { get; set; }
```

**Type** `string`

#### Health

```csharp
[JsonInclude]
[JsonPropertyName("Health")]
public string Health { get; set; }
```

**Type** `string`

#### PhysNotes

```csharp
[JsonInclude]
[JsonPropertyName("PhysNotes")]
public string PhysNotes { get; set; }
```

**Type** `string`

#### Appearance

```csharp
[JsonInclude]
[JsonPropertyName("Appearance")]
public string Appearance { get; set; }
```

**Type** `string`

#### RelationshipList

```csharp
[JsonInclude]
[JsonPropertyName("RelationshipList")]
public List<RelationshipModel> RelationshipList { get; set; }
```

**Type** `List<RelationshipModel>`

#### Economic

```csharp
[JsonInclude]
[JsonPropertyName("Economic")]
public string Economic { get; set; }
```

**Type** `string`

#### Education

```csharp
[JsonInclude]
[JsonPropertyName("Education")]
public string Education { get; set; }
```

**Type** `string`

#### Ethnic

```csharp
[JsonInclude]
[JsonPropertyName("Ethnic")]
public string Ethnic { get; set; }
```

**Type** `string`

#### Religion

```csharp
[JsonInclude]
[JsonPropertyName("Religion")]
public string Religion { get; set; }
```

**Type** `string`

#### Enneagram

```csharp
[JsonInclude]
[JsonPropertyName("Enneagram")]
public string Enneagram { get; set; }
```

**Type** `string`

#### Intelligence

```csharp
[JsonInclude]
[JsonPropertyName("Intelligence")]
public string Intelligence { get; set; }
```

**Type** `string`

#### Values

```csharp
[JsonInclude]
[JsonPropertyName("Values")]
public string Values { get; set; }
```

**Type** `string`

#### Abnormality

```csharp
[JsonInclude]
[JsonPropertyName("Abnormality")]
public string Abnormality { get; set; }
```

**Type** `string`

#### Focus

```csharp
[JsonInclude]
[JsonPropertyName("Focus")]
public string Focus { get; set; }
```

**Type** `string`

#### PsychNotes

```csharp
[JsonInclude]
[JsonPropertyName("PsychNotes")]
public string PsychNotes { get; set; }
```

**Type** `string`

#### Adventureousness

```csharp
[JsonInclude]
[JsonPropertyName("Adventureousness")]
public string Adventureousness { get; set; }
```

**Type** `string`

#### Aggression

```csharp
[JsonInclude]
[JsonPropertyName("Aggression")]
public string Aggression { get; set; }
```

**Type** `string`

#### Confidence

```csharp
[JsonInclude]
[JsonPropertyName("Confidence")]
public string Confidence { get; set; }
```

**Type** `string`

#### Conscientiousness

```csharp
[JsonInclude]
[JsonPropertyName("Conscientiousness")]
public string Conscientiousness { get; set; }
```

**Type** `string`

#### Creativity

```csharp
[JsonInclude]
[JsonPropertyName("Creativity")]
public string Creativity { get; set; }
```

**Type** `string`

#### Dominance

```csharp
[JsonInclude]
[JsonPropertyName("Dominance")]
public string Dominance { get; set; }
```

**Type** `string`

#### Enthusiasm

```csharp
[JsonInclude]
[JsonPropertyName("Enthusiasm")]
public string Enthusiasm { get; set; }
```

**Type** `string`

#### Assurance

```csharp
[JsonInclude]
[JsonPropertyName("Assurance")]
public string Assurance { get; set; }
```

**Type** `string`

#### Sensitivity

```csharp
[JsonInclude]
[JsonPropertyName("Sensitivity")]
public string Sensitivity { get; set; }
```

**Type** `string`

#### Shrewdness

```csharp
[JsonInclude]
[JsonPropertyName("Shrewdness")]
public string Shrewdness { get; set; }
```

**Type** `string`

#### Sociability

```csharp
[JsonInclude]
[JsonPropertyName("Sociability")]
public string Sociability { get; set; }
```

**Type** `string`

#### Stability

```csharp
[JsonInclude]
[JsonPropertyName("Stability")]
public string Stability { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

#### TraitList

```csharp
[JsonInclude]
[JsonPropertyName("TraitList")]
public List<string> TraitList { get; set; }
```

**Type** `List<string>`

#### Flaw

```csharp
[JsonInclude]
[JsonPropertyName("Flaw")]
public string Flaw { get; set; }
```

**Type** `string`

#### BackStory

```csharp
[JsonInclude]
[JsonPropertyName("BackStory")]
public string BackStory { get; set; }
```

**Type** `string`

---

## FolderModel

*Class*: `StoryCADLib.Models.FolderModel`

```csharp
public class FolderModel : StoryElement
```

### Constructors

#### FolderModel(string, StoryModel, StoryItemType, StoryNodeItem)

```csharp
public FolderModel(string name, StoryModel model, StoryItemType type, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `type` (`StoryItemType`)
- `Node` (`StoryNodeItem`)

#### FolderModel()

Json constructor

```csharp
public FolderModel()
```

---

## OverviewModel

*Class*: `StoryCADLib.Models.OverviewModel`

OverviewModel contains overview information for the entire story, such as title, author, and so on.
It's a good place to capture the original idea which prompted the story.
There is only one OverviewModel instance for each story. It's also the root of the Shell Page's
StoryExplorer TreeView.

```csharp
public class OverviewModel : StoryElement
```

### Constructors

#### OverviewModel(string, StoryModel, StoryNodeItem)

```csharp
public OverviewModel(string name, StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### OverviewModel()

JSON Constructor.

```csharp
public OverviewModel()
```

### Properties

#### DateCreated

```csharp
[JsonInclude]
[JsonPropertyName("DateCreated")]
public string DateCreated { get; set; }
```

**Type** `string`

#### Author

```csharp
[JsonInclude]
[JsonPropertyName("Author")]
public string Author { get; set; }
```

**Type** `string`

#### DateModified

```csharp
[JsonInclude]
[JsonPropertyName("DateModified")]
public string DateModified { get; set; }
```

**Type** `string`

#### Concept

```csharp
[JsonInclude]
[JsonPropertyName("Concept")]
public string Concept { get; set; }
```

**Type** `string`

#### StoryProblem

```csharp
[JsonInclude]
[JsonPropertyName("StoryProblem")]
public Guid StoryProblem { get; set; }
```

**Type** `Guid`

#### Premise

```csharp
[JsonInclude]
[JsonPropertyName("Premise")]
public string Premise { get; set; }
```

**Type** `string`

#### StoryType

```csharp
[JsonInclude]
[JsonPropertyName("StoryType")]
public string StoryType { get; set; }
```

**Type** `string`

#### StoryGenre

```csharp
[JsonInclude]
[JsonPropertyName("StoryGenre")]
public string StoryGenre { get; set; }
```

**Type** `string`

#### Viewpoint

```csharp
[JsonInclude]
[JsonPropertyName("Viewpoint")]
public string Viewpoint { get; set; }
```

**Type** `string`

#### ViewpointCharacter

```csharp
[JsonInclude]
[JsonPropertyName("ViewpointCharacter")]
public Guid ViewpointCharacter { get; set; }
```

**Type** `Guid`

#### Voice

```csharp
[JsonInclude]
[JsonPropertyName("Voice")]
public string Voice { get; set; }
```

**Type** `string`

#### LiteraryDevice

```csharp
[JsonInclude]
[JsonPropertyName("LiteraryDevice")]
public string LiteraryDevice { get; set; }
```

**Type** `string`

#### Tense

```csharp
[JsonInclude]
[JsonPropertyName("Tense")]
public string Tense { get; set; }
```

**Type** `string`

#### Style

```csharp
[JsonInclude]
[JsonPropertyName("Style")]
public string Style { get; set; }
```

**Type** `string`

#### StructureNotes

```csharp
[JsonInclude]
[JsonPropertyName("StructureNotes")]
public string StructureNotes { get; set; }
```

**Type** `string`

#### Tone

```csharp
[JsonInclude]
[JsonPropertyName("Tone")]
public string Tone { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

---

## ProblemModel

*Class*: `StoryCADLib.Models.ProblemModel`

```csharp
public class ProblemModel : StoryElement
```

### Constructors

#### ProblemModel(StoryModel, StoryNodeItem)

```csharp
public ProblemModel(StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### ProblemModel(string, StoryModel, StoryNodeItem)

```csharp
public ProblemModel(string name, StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### ProblemModel()

```csharp
public ProblemModel()
```

### Properties

#### ProblemType

```csharp
[JsonInclude]
[JsonPropertyName("ProblemType")]
public string ProblemType { get; set; }
```

**Type** `string`

#### ConflictType

```csharp
[JsonInclude]
[JsonPropertyName("ConflictType")]
public string ConflictType { get; set; }
```

**Type** `string`

#### ProblemCategory

```csharp
[JsonInclude]
[JsonPropertyName("ProblemCategory")]
public string ProblemCategory { get; set; }
```

**Type** `string`

#### Subject

```csharp
[JsonInclude]
[JsonPropertyName("Subject")]
public string Subject { get; set; }
```

**Type** `string`

#### ProblemSource

```csharp
[JsonInclude]
[JsonPropertyName("ProblemSource")]
public string ProblemSource { get; set; }
```

**Type** `string`

#### Protagonist

```csharp
[JsonInclude]
[JsonPropertyName("Protagonist")]
public Guid Protagonist { get; set; }
```

**Type** `Guid`

#### ProtGoal

```csharp
[JsonInclude]
[JsonPropertyName("ProtGoal")]
public string ProtGoal { get; set; }
```

**Type** `string`

#### ProtMotive

```csharp
[JsonInclude]
[JsonPropertyName("ProtMotive")]
public string ProtMotive { get; set; }
```

**Type** `string`

#### ProtConflict

```csharp
[JsonInclude]
[JsonPropertyName("ProtConflict")]
public string ProtConflict { get; set; }
```

**Type** `string`

#### Antagonist

```csharp
[JsonInclude]
[JsonPropertyName("Antagonist")]
public Guid Antagonist { get; set; }
```

**Type** `Guid`

#### AntagGoal

```csharp
[JsonInclude]
[JsonPropertyName("AntagGoal")]
public string AntagGoal { get; set; }
```

**Type** `string`

#### AntagMotive

```csharp
[JsonInclude]
[JsonPropertyName("AntagMotive")]
public string AntagMotive { get; set; }
```

**Type** `string`

#### AntagConflict

```csharp
[JsonInclude]
[JsonPropertyName("AntagConflict")]
public string AntagConflict { get; set; }
```

**Type** `string`

#### Outcome

```csharp
[JsonInclude]
[JsonPropertyName("Outcome")]
public string Outcome { get; set; }
```

**Type** `string`

#### Method

```csharp
[JsonInclude]
[JsonPropertyName("Method")]
public string Method { get; set; }
```

**Type** `string`

#### Theme

```csharp
[JsonInclude]
[JsonPropertyName("Theme")]
public string Theme { get; set; }
```

**Type** `string`

#### Premise

```csharp
[JsonInclude]
[JsonPropertyName("Premise")]
public string Premise { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

#### StructureTitle

Name of StructureBeatsModel used in structure tab

```csharp
[JsonInclude]
[JsonPropertyName("StructureTitle")]
public string StructureTitle { get; set; }
```

**Type** `string`

#### StructureDescription

Description of StructureBeatsModel used in structure tab

```csharp
[JsonInclude]
[JsonPropertyName("StructureDescription")]
public string StructureDescription { get; set; }
```

**Type** `string`

#### StructureBeats

Beat nodes of the structure

```csharp
[JsonInclude]
[JsonPropertyName("StructureBeats")]
public ObservableCollection<StructureBeatViewModel> StructureBeats { get; set; }
```

**Type** `ObservableCollection<StructureBeatViewModel>`

#### BoundStructure

A problem cannot be bound to more than one structure

```csharp
[JsonInclude]
[JsonPropertyName("BoundStructure")]
public string BoundStructure { get; set; }
```

**Type** `string`

---

## RelationshipModel

*Class*: `StoryCADLib.Models.RelationshipModel`

```csharp
public class RelationshipModel
```

### Constructors

#### RelationshipModel(Guid, string)

```csharp
public RelationshipModel(Guid partnerUuid, string type)
```

**Parameters**

- `partnerUuid` (`Guid`)
- `type` (`string`)

#### RelationshipModel()

```csharp
public RelationshipModel()
```

### Fields

#### CharVM

```csharp
[JsonIgnore]
public readonly CharacterViewModel CharVM
```

**Type** `CharacterViewModel`

### Properties

#### Partner

```csharp
[JsonIgnore]
public StoryElement Partner { get; set; }
```

**Type** `StoryElement`

#### PartnerUuid

```csharp
[JsonInclude]
[JsonPropertyName("PartnerUuid")]
public Guid PartnerUuid { get; set; }
```

**Type** `Guid`

#### RelationType

```csharp
[JsonInclude]
[JsonPropertyName("RelationType")]
public string RelationType { get; set; }
```

**Type** `string`

#### Trait

```csharp
[JsonInclude]
[JsonPropertyName("Trait")]
public string Trait { get; set; }
```

**Type** `string`

#### Attitude

```csharp
[JsonInclude]
[JsonPropertyName("Attitude")]
public string Attitude { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

---

## SceneModel

*Class*: `StoryCADLib.Models.SceneModel`

```csharp
public class SceneModel : StoryElement
```

### Constructors

#### SceneModel(StoryModel, StoryNodeItem)

```csharp
public SceneModel(StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### SceneModel(string, StoryModel, StoryNodeItem)

```csharp
public SceneModel(string name, StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### SceneModel()

```csharp
public SceneModel()
```

### Properties

#### SceneDescription

```csharp
[JsonInclude]
[JsonPropertyName("Description")]
public string SceneDescription { get; set; }
```

**Type** `string`

#### ViewpointCharacter

```csharp
[JsonInclude]
[JsonPropertyName("ViewpointCharacter")]
public Guid ViewpointCharacter { get; set; }
```

**Type** `Guid`

#### Date

```csharp
[JsonInclude]
[JsonPropertyName("Date")]
public string Date { get; set; }
```

**Type** `string`

#### Time

```csharp
[JsonInclude]
[JsonPropertyName("Time")]
public string Time { get; set; }
```

**Type** `string`

#### Setting

```csharp
[JsonInclude]
[JsonPropertyName("Setting")]
public Guid Setting { get; set; }
```

**Type** `Guid`

#### SceneType

```csharp
[JsonInclude]
[JsonPropertyName("SceneType")]
public string SceneType { get; set; }
```

**Type** `string`

#### CastMembers

```csharp
[JsonInclude]
[JsonPropertyName("CastMembers")]
public List<Guid> CastMembers { get; set; }
```

**Type** `List<Guid>`

#### Protagonist

```csharp
[JsonInclude]
[JsonPropertyName("Protagonist")]
public Guid Protagonist { get; set; }
```

**Type** `Guid`

#### ProtagEmotion

```csharp
[JsonInclude]
[JsonPropertyName("ProtagEmotion")]
public string ProtagEmotion { get; set; }
```

**Type** `string`

#### ProtagGoal

```csharp
[JsonInclude]
[JsonPropertyName("ProtagGoal")]
public string ProtagGoal { get; set; }
```

**Type** `string`

#### Antagonist

```csharp
[JsonInclude]
[JsonPropertyName("Antagonist")]
public Guid Antagonist { get; set; }
```

**Type** `Guid`

#### AntagEmotion

```csharp
[JsonInclude]
[JsonPropertyName("AntagEmotion")]
public string AntagEmotion { get; set; }
```

**Type** `string`

#### AntagGoal

```csharp
[JsonInclude]
[JsonPropertyName("AntagGoal")]
public string AntagGoal { get; set; }
```

**Type** `string`

#### Opposition

```csharp
[JsonInclude]
[JsonPropertyName("Opposition")]
public string Opposition { get; set; }
```

**Type** `string`

#### Outcome

```csharp
[JsonInclude]
[JsonPropertyName("Outcome")]
public string Outcome { get; set; }
```

**Type** `string`

#### ScenePurpose

```csharp
[JsonInclude]
[JsonPropertyName("ScenePurpose")]
public List<string> ScenePurpose { get; set; }
```

**Type** `List<string>`

#### ValueExchange

```csharp
[JsonInclude]
[JsonPropertyName("ValueExchange")]
public string ValueExchange { get; set; }
```

**Type** `string`

#### Events

```csharp
[JsonInclude]
[JsonPropertyName("Events")]
public string Events { get; set; }
```

**Type** `string`

#### Consequences

```csharp
[JsonInclude]
[JsonPropertyName("Consequences")]
public string Consequences { get; set; }
```

**Type** `string`

#### Significance

```csharp
[JsonInclude]
[JsonPropertyName("Significance")]
public string Significance { get; set; }
```

**Type** `string`

#### Realization

```csharp
[JsonInclude]
[JsonPropertyName("Realization")]
public string Realization { get; set; }
```

**Type** `string`

#### Emotion

```csharp
[JsonInclude]
[JsonPropertyName("Emotion")]
public string Emotion { get; set; }
```

**Type** `string`

#### NewGoal

```csharp
[JsonInclude]
[JsonPropertyName("NewGoal")]
public string NewGoal { get; set; }
```

**Type** `string`

#### Review

```csharp
[JsonInclude]
[JsonPropertyName("Review")]
public string Review { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

---

## SettingModel

*Class*: `StoryCADLib.Models.SettingModel`

```csharp
public class SettingModel : StoryElement
```

### Constructors

#### SettingModel(StoryModel, StoryNodeItem)

```csharp
public SettingModel(StoryModel model, StoryNodeItem node)
```

**Parameters**

- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

#### SettingModel(string, StoryModel, StoryNodeItem)

```csharp
public SettingModel(string name, StoryModel model, StoryNodeItem Node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `Node` (`StoryNodeItem`)

#### SettingModel()

```csharp
public SettingModel()
```

### Fields

#### SettingNames

```csharp
[JsonIgnore]
public static ObservableCollection<string> SettingNames
```

**Type** `ObservableCollection<string>`

### Properties

#### Locale

```csharp
[JsonInclude]
[JsonPropertyName("Locale")]
public string Locale { get; set; }
```

**Type** `string`

#### Season

```csharp
[JsonInclude]
[JsonPropertyName("Season")]
public string Season { get; set; }
```

**Type** `string`

#### Period

```csharp
[JsonInclude]
[JsonPropertyName("Period")]
public string Period { get; set; }
```

**Type** `string`

#### Lighting

```csharp
[JsonInclude]
[JsonPropertyName("Lighting")]
public string Lighting { get; set; }
```

**Type** `string`

#### Weather

```csharp
[JsonInclude]
[JsonPropertyName("Weather")]
public string Weather { get; set; }
```

**Type** `string`

#### Temperature

```csharp
[JsonInclude]
[JsonPropertyName("Temperature")]
public string Temperature { get; set; }
```

**Type** `string`

#### Props

```csharp
[JsonInclude]
[JsonPropertyName("Props")]
public string Props { get; set; }
```

**Type** `string`

#### Sights

```csharp
[JsonInclude]
[JsonPropertyName("Sights")]
public string Sights { get; set; }
```

**Type** `string`

#### Sounds

```csharp
[JsonInclude]
[JsonPropertyName("Sounds")]
public string Sounds { get; set; }
```

**Type** `string`

#### Touch

```csharp
[JsonInclude]
[JsonPropertyName("Touch")]
public string Touch { get; set; }
```

**Type** `string`

#### SmellTaste

```csharp
[JsonInclude]
[JsonPropertyName("SmellTaste")]
public string SmellTaste { get; set; }
```

**Type** `string`

#### Notes

```csharp
[JsonInclude]
[JsonPropertyName("Notes")]
public string Notes { get; set; }
```

**Type** `string`

---

## StoryDocument

*Class*: `StoryCADLib.Models.StoryDocument`

Encapsulates a story document, combining the model and its file path.
This ensures the model and path are always kept together as a unit.

```csharp
public sealed class StoryDocument
```

### Constructors

#### StoryDocument(StoryModel, string?)

Creates a new StoryDocument with the specified model and optional file path.

```csharp
public StoryDocument(StoryModel model, string? filePath = null)
```

**Parameters**

- `model` (`StoryModel`): The story model (required)
- `filePath` (`string`): The file path (optional, null for new documents)

### Properties

#### Model

The story model containing all story data.
Readonly - to change models, create a new StoryDocument instance.

```csharp
public StoryModel Model { get; }
```

**Type** `StoryModel`

#### FilePath

The file path where this document is saved.
Null for new unsaved documents ("Untitled").
Mutable to support SaveAs operations.

```csharp
public string? FilePath { get; set; }
```

**Type** `string`

#### IsDirty

Indicates whether the document has unsaved changes.
Delegates to the Model's Changed property.

```csharp
public bool IsDirty { get; }
```

**Type** `bool`

---

## StoryElement

*Class*: `StoryCADLib.Models.StoryElement`

```csharp
public class StoryElement : ObservableObject
```

### Constructors

#### StoryElement(string, StoryItemType, StoryModel, StoryNodeItem)

Creates a new story element

```csharp
public StoryElement(string name, StoryItemType type, StoryModel model, StoryNodeItem parentNode)
```

**Parameters**

- `name` (`string`): Name of element
- `type` (`StoryItemType`): Type of element
- `model` (`StoryModel`): Story Model this element belongs to
- `parentNode` (`StoryNodeItem`): Parent of this node

#### StoryElement()

Parameterless constructor for JSON Deserialization.
Don't remove.

```csharp
public StoryElement()
```

### Properties

#### Uuid

```csharp
[JsonInclude]
[JsonPropertyName("GUID")]
public Guid Uuid { get; set; }
```

**Type** `Guid`

#### Description

Common description field that is mapped to the main textbox on an element.

```csharp
[JsonInclude]
[JsonPropertyName("ElementDescription")]
public string Description { get; set; }
```

**Type** `string`

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### ElementType

```csharp
[JsonInclude]
[JsonPropertyName("Type")]
public StoryItemType ElementType { get; set; }
```

**Type** `StoryItemType`

#### Node

```csharp
[JsonIgnore]
public StoryNodeItem Node { get; set; }
```

**Type** `StoryNodeItem`

#### IsSelected

```csharp
[JsonIgnore]
public bool IsSelected { get; set; }
```

**Type** `bool`

### Methods

#### GetByGuid(Guid, StoryModel)

Retrieve a StoryElement from its Guid.
Guids are used as keys to StoryElements, stored in
the StoryModel's StoryElementCollection. They also
identify links from one StoryElement to another,
such as the Setting or a cast member Character in
a Scene. We use Guid.Empty as the value for such a
link  until it's assigned.
These placeholder links are often expected to be a
StoryElement key, such as to display the name of
the setting on the Scene Content pane. Treating
Guid.Empty as an 'Undefined' StoryElement, with
a blank name, simplifies that code.

```csharp
public static StoryElement GetByGuid(Guid guid, StoryModel storyModel = null)
```

**Parameters**

- `guid` (`Guid`): The Guid of the StoryElement to retrieve
- `storyModel` (`StoryModel`): optional story model override, defaults to current app state model.

**Returns** `StoryElement`

#### Deserialize(string)

Deserializes a JSON string into a StoryElement.

```csharp
public static StoryElement Deserialize(string json)
```

**Parameters**

- `json` (`string`): JSON to deserialize.

**Returns** `StoryElement`: StoryElement Object.

#### Serialize()

Serialises this StoryElement into JSON.

```csharp
public string Serialize()
```

**Returns** `string`: JSON Representation of this object.

#### ToString()

```csharp
public override string ToString()
```

**Returns** `string`

---

## StoryElementCollection

*Class*: `StoryCADLib.Models.StoryElementCollection`

StoryElementCollection is an ObservableCollection of StoryElement
instances, which automatically maintains several derivative
collections when StoryElementCollection has elements added or
removed.

```csharp
public class StoryElementCollection : ObservableCollection<StoryElement>
```

### Constructors

#### StoryElementCollection()

```csharp
public StoryElementCollection()
```

### Fields

#### Characters

```csharp
public ObservableCollection<StoryElement> Characters
```

**Type** `ObservableCollection<StoryElement>`

#### Scenes

```csharp
public ObservableCollection<StoryElement> Scenes
```

**Type** `ObservableCollection<StoryElement>`

#### Settings

```csharp
public ObservableCollection<StoryElement> Settings
```

**Type** `ObservableCollection<StoryElement>`

#### StoryElementGuids

```csharp
public Dictionary<Guid, StoryElement> StoryElementGuids
```

**Type** `Dictionary<Guid, StoryElement>`

### Properties

#### Problems

```csharp
public ObservableCollection<StoryElement> Problems { get; }
```

**Type** `ObservableCollection<StoryElement>`

---

## StoryItemType

*Enum*: `StoryCADLib.Models.StoryItemType`

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StoryItemType
```

### Values

| Value | Description |
|-------|-------------|
| `StoryOverview` |  |
| `Problem` |  |
| `Character` |  |
| `Setting` |  |
| `Scene` |  |
| `Folder` |  |
| `Section` |  |
| `Web` |  |
| `Notes` |  |
| `TrashCan` |  |
| `StoryWorld` |  |
| `Unknown` |  |

---

## StoryModel

*Class*: `StoryCADLib.Models.StoryModel`

```csharp
public class StoryModel : ObservableObject
```

### Constructors

#### StoryModel()

```csharp
public StoryModel()
```

### Fields

#### FirstVersion

The first version of StoryCAD that this file was created with.

```csharp
[JsonInclude]
[JsonPropertyName("CreatedVersion")]
public string FirstVersion
```

**Type** `string`

#### LastVersion

The last version of StoryCAD that this file was saved with.

```csharp
[JsonInclude]
[JsonPropertyName("LastVersion")]
public string LastVersion
```

**Type** `string`

#### StoryElements

```csharp
[JsonInclude]
[JsonPropertyName("Elements")]
public StoryElementCollection StoryElements
```

**Type** `StoryElementCollection`

#### ExplorerView

```csharp
[JsonIgnore]
public ObservableCollection<StoryNodeItem> ExplorerView
```

**Type** `ObservableCollection<StoryNodeItem>`

#### NarratorView

```csharp
[JsonIgnore]
public ObservableCollection<StoryNodeItem> NarratorView
```

**Type** `ObservableCollection<StoryNodeItem>`

### Properties

#### Changed

```csharp
[JsonIgnore]
public bool Changed { get; set; }
```

**Type** `bool`

#### CurrentView

```csharp
[JsonIgnore]
public ObservableCollection<StoryNodeItem> CurrentView { get; set; }
```

**Type** `ObservableCollection<StoryNodeItem>`

#### TrashView

```csharp
[JsonIgnore]
public ObservableCollection<StoryNodeItem> TrashView { get; set; }
```

**Type** `ObservableCollection<StoryNodeItem>`

#### CurrentViewType

Current view type tracking

```csharp
[JsonIgnore]
public StoryViewType CurrentViewType { get; set; }
```

**Type** `StoryViewType`

### Methods

#### Serialize()

Serialises the model to JSON

```csharp
public string Serialize()
```

**Returns** `string`

#### RefreshCurrentView()

Forces the UI to refresh by re-binding CurrentView.
Used after Collaborator updates elements via API.

```csharp
public void RefreshCurrentView()
```

---

## StoryWorldModel

*Class*: `StoryCADLib.Models.StoryWorldModel`

Model for the StoryWorld story element.
Contains worldbuilding information organized by category.
Single instance per story (like StoryOverview), but optional.

```csharp
public class StoryWorldModel : StoryElement
```

### Constructors

#### StoryWorldModel(StoryModel, StoryNodeItem)

Creates a new StoryWorld with default name.

```csharp
public StoryWorldModel(StoryModel model, StoryNodeItem node)
```

**Parameters**

- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

#### StoryWorldModel(string, StoryModel, StoryNodeItem)

Creates a new StoryWorld with specified name.

```csharp
public StoryWorldModel(string name, StoryModel model, StoryNodeItem node)
```

**Parameters**

- `name` (`string`)
- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

#### StoryWorldModel()

Parameterless constructor for JSON deserialization.

```csharp
public StoryWorldModel()
```

### Properties

#### WorldType

```csharp
[JsonInclude]
[JsonPropertyName("WorldType")]
public string WorldType { get; set; }
```

**Type** `string`

#### Ontology

```csharp
[JsonInclude]
[JsonPropertyName("Ontology")]
public string Ontology { get; set; }
```

**Type** `string`

#### WorldRelation

```csharp
[JsonInclude]
[JsonPropertyName("WorldRelation")]
public string WorldRelation { get; set; }
```

**Type** `string`

#### RuleTransparency

```csharp
[JsonInclude]
[JsonPropertyName("RuleTransparency")]
public string RuleTransparency { get; set; }
```

**Type** `string`

#### ScaleOfDifference

```csharp
[JsonInclude]
[JsonPropertyName("ScaleOfDifference")]
public string ScaleOfDifference { get; set; }
```

**Type** `string`

#### AgencySource

```csharp
[JsonInclude]
[JsonPropertyName("AgencySource")]
public string AgencySource { get; set; }
```

**Type** `string`

#### ToneLogic

```csharp
[JsonInclude]
[JsonPropertyName("ToneLogic")]
public string ToneLogic { get; set; }
```

**Type** `string`

#### PhysicalWorlds

```csharp
[JsonInclude]
[JsonPropertyName("PhysicalWorlds")]
public List<PhysicalWorldEntry> PhysicalWorlds { get; set; }
```

**Type** `List<PhysicalWorldEntry>`

#### Species

```csharp
[JsonInclude]
[JsonPropertyName("Species")]
public List<SpeciesEntry> Species { get; set; }
```

**Type** `List<SpeciesEntry>`

#### Cultures

```csharp
[JsonInclude]
[JsonPropertyName("Cultures")]
public List<CultureEntry> Cultures { get; set; }
```

**Type** `List<CultureEntry>`

#### Governments

```csharp
[JsonInclude]
[JsonPropertyName("Governments")]
public List<GovernmentEntry> Governments { get; set; }
```

**Type** `List<GovernmentEntry>`

#### Religions

```csharp
[JsonInclude]
[JsonPropertyName("Religions")]
public List<ReligionEntry> Religions { get; set; }
```

**Type** `List<ReligionEntry>`

#### FoundingEvents

```csharp
[JsonInclude]
[JsonPropertyName("FoundingEvents")]
public string FoundingEvents { get; set; }
```

**Type** `string`

#### MajorConflicts

```csharp
[JsonInclude]
[JsonPropertyName("MajorConflicts")]
public string MajorConflicts { get; set; }
```

**Type** `string`

#### Eras

```csharp
[JsonInclude]
[JsonPropertyName("Eras")]
public string Eras { get; set; }
```

**Type** `string`

#### TechnologicalShifts

```csharp
[JsonInclude]
[JsonPropertyName("TechnologicalShifts")]
public string TechnologicalShifts { get; set; }
```

**Type** `string`

#### LostKnowledge

```csharp
[JsonInclude]
[JsonPropertyName("LostKnowledge")]
public string LostKnowledge { get; set; }
```

**Type** `string`

#### EconomicSystem

```csharp
[JsonInclude]
[JsonPropertyName("EconomicSystem")]
public string EconomicSystem { get; set; }
```

**Type** `string`

#### Currency

```csharp
[JsonInclude]
[JsonPropertyName("Currency")]
public string Currency { get; set; }
```

**Type** `string`

#### TradeRoutes

```csharp
[JsonInclude]
[JsonPropertyName("TradeRoutes")]
public string TradeRoutes { get; set; }
```

**Type** `string`

#### Professions

```csharp
[JsonInclude]
[JsonPropertyName("Professions")]
public string Professions { get; set; }
```

**Type** `string`

#### WealthDistribution

```csharp
[JsonInclude]
[JsonPropertyName("WealthDistribution")]
public string WealthDistribution { get; set; }
```

**Type** `string`

#### SystemType

```csharp
[JsonInclude]
[JsonPropertyName("SystemType")]
public string SystemType { get; set; }
```

**Type** `string`

#### Source

```csharp
[JsonInclude]
[JsonPropertyName("Source")]
public string Source { get; set; }
```

**Type** `string`

#### Rules

```csharp
[JsonInclude]
[JsonPropertyName("Rules")]
public string Rules { get; set; }
```

**Type** `string`

#### Limitations

```csharp
[JsonInclude]
[JsonPropertyName("Limitations")]
public string Limitations { get; set; }
```

**Type** `string`

#### Cost

```csharp
[JsonInclude]
[JsonPropertyName("Cost")]
public string Cost { get; set; }
```

**Type** `string`

#### Practitioners

```csharp
[JsonInclude]
[JsonPropertyName("Practitioners")]
public string Practitioners { get; set; }
```

**Type** `string`

#### SocialImpact

```csharp
[JsonInclude]
[JsonPropertyName("SocialImpact")]
public string SocialImpact { get; set; }
```

**Type** `string`

---

## TrashCanModel

*Class*: `StoryCADLib.Models.TrashCanModel`

The TrashCanModel is a container for deleted StoryElements. It's the second root node
in both the Explorer View and Narrator Views, and contains no properties.

```csharp
public class TrashCanModel : StoryElement
```

### Constructors

#### TrashCanModel()

JSON Constructor

```csharp
public TrashCanModel()
```

#### TrashCanModel(StoryModel, StoryNodeItem)

```csharp
public TrashCanModel(StoryModel model, StoryNodeItem node)
```

**Parameters**

- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

---

## WebModel

*Class*: `StoryCADLib.Models.WebModel`

```csharp
public class WebModel : StoryElement
```

### Constructors

#### WebModel(StoryModel, StoryNodeItem)

```csharp
public WebModel(StoryModel model, StoryNodeItem node)
```

**Parameters**

- `model` (`StoryModel`)
- `node` (`StoryNodeItem`)

#### WebModel()

```csharp
public WebModel()
```

### Fields

#### Timestamp

```csharp
[JsonInclude]
[JsonPropertyName("Timestamp")]
public DateTime Timestamp
```

**Type** `DateTime`

#### URL

```csharp
[JsonInclude]
[JsonPropertyName("URI")]
public Uri URL
```

**Type** `Uri`
