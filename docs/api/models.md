---
layout: default
title: Models
parent: API Reference
nav_order: 3
---

# Models

Auto-generated reference for the `StoryCADLib.Models` namespace (27 types).

{: .fs-6 .fw-300 }

## AppState

*Class* — `StoryCADLib.Models.AppState`

This class holds developer tools and app data.

```csharp
public class AppState
```

### Constructors

#### AppState()

```csharp
public AppState()
```

### Fields

#### EnvPresent

Is .env present?

```csharp
public bool EnvPresent
```

**Type** `bool`

#### Headless

Suppresses graphical output if True

```csharp
public bool Headless
```

**Type** `bool`

#### IsClosing

Indicates the application is in the process of shutting down.
Used to guard UI operations that would fail after window destruction.

```csharp
public bool IsClosing
```

**Type** `bool`

#### LoadedWithVersionChange

Returns true if the app has loaded with a version change.
If this is true a changelog will show, install service will
run and the server will update the version.

```csharp
public bool LoadedWithVersionChange
```

**Type** `bool`

#### StartUpTimer

This is a debug timer that counts the amount of time from
the app being opened to Shell being properly initialised.

```csharp
public readonly Stopwatch StartUpTimer
```

**Type** `Stopwatch`

### Properties

#### RootDirectory

This is the path where all app files are stored

```csharp
public string RootDirectory { get; }
```

**Type** `string`

#### DeveloperBuild

This variable will return true if any of following are true:
- The Build revision is NOT 0.
- A debugger i.e. VS2022 is attached.
- .ENV is missing.
Usually it's all or none of the above.

```csharp
public bool DeveloperBuild { get; }
```

**Type** `bool`

#### Version

The current version of StoryCADLib

```csharp
public string Version { get; }
```

**Type** `string`

#### CurrentDocument

```csharp
public StoryDocument? CurrentDocument { get; set; }
```

**Type** `StoryDocument`

#### CurrentSaveable

The current ViewModel that can save its edits back to the model.
Set by pages in OnNavigatedTo when they have saveable content.
Null for pages without editable content (Home, Reports, etc.).

```csharp
public ISaveable? CurrentSaveable { get; set; }
```

**Type** `ISaveable`

#### CurrentViewType

The current view type (Explorer or Narrator).
Set by ShellViewModel.ViewChanged() when user switches views.

```csharp
public StoryViewType CurrentViewType { get; set; }
```

**Type** `StoryViewType`

#### CurrentNode

The currently selected node in the tree view.
Set by ShellViewModel.TreeViewNodeClicked() when user clicks a node.
Null when no node is selected.

```csharp
public StoryNodeItem? CurrentNode { get; set; }
```

**Type** `StoryNodeItem`

#### RightTappedNode

The node that was right-clicked to open a context menu.
Set by Shell.xaml.cs right-click handler.
Used by tools that operate on the right-clicked node.
Null when no node has been right-clicked.

```csharp
public StoryNodeItem? RightTappedNode { get; set; }
```

**Type** `StoryNodeItem`

### Events

#### CurrentDocumentChanged

The currently open story document, combining the model and its file path.
Null when no document is open (app startup).
When set, triggers UI binding updates through the Shell.

```csharp
public event EventHandler? CurrentDocumentChanged
```

**Type** `EventHandler`

---

## BrowserType

*Enum* — `StoryCADLib.Models.BrowserType`

```csharp
public enum BrowserType
```

### Values

| Value | Description |
|-------|-------------|
| `DuckDuckGo` |  |
| `Google` |  |
| `Bing` |  |
| `Yahoo` |  |

---

## CharacterModel

*Class* — `StoryCADLib.Models.CharacterModel`

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

## FilePathAttribute

*Class* — `StoryCADLib.Models.FilePathAttribute`

```csharp
public class FilePathAttribute : ValidationAttribute
```

### Constructors

#### FilePathAttribute()

```csharp
public FilePathAttribute()
```

### Methods

#### IsValid(object, ValidationContext)

```csharp
protected override ValidationResult IsValid(object value, ValidationContext validationContext)
```

**Parameters**

- `value` (`object`)
- `validationContext` (`ValidationContext`)

**Returns** `ValidationResult`

---

## FolderModel

*Class* — `StoryCADLib.Models.FolderModel`

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

## ListData

*Class* — `StoryCADLib.Models.ListData`

This stores the lists for StoryCAD's Lists.json.
Previously lists were stored in GlobalData.

```csharp
public class ListData
```

### Constructors

#### ListData(ILogService, JSONResourceLoader)

```csharp
public ListData(ILogService log, JSONResourceLoader resourceLoader)
```

**Parameters**

- `log` (`ILogService`)
- `resourceLoader` (`JSONResourceLoader`)

### Fields

#### ListControlSource

```csharp
public Dictionary<string, ObservableCollection<string>> ListControlSource
```

**Type** `Dictionary<string, ObservableCollection<string>>`

---

## OverviewModel

*Class* — `StoryCADLib.Models.OverviewModel`

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

## PersistableNode

*Class* — `StoryCADLib.Models.PersistableNode`

Node that can be persisted to JSON

```csharp
public class PersistableNode
```

### Constructors

#### PersistableNode()

```csharp
public PersistableNode()
```

### Properties

#### Uuid

UUID of node

```csharp
public Guid Uuid { get; set; }
```

**Type** `Guid`

#### ParentUuid

UUID of parent node

```csharp
public Guid? ParentUuid { get; set; }
```

**Type** `Guid?`

---

## ProblemModel

*Class* — `StoryCADLib.Models.ProblemModel`

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

*Class* — `StoryCADLib.Models.RelationshipModel`

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

## RelationType

*Class* — `StoryCADLib.Models.RelationType`

```csharp
public class RelationType : ObservableObject
```

### Constructors

#### RelationType(string, string)

```csharp
public RelationType(string memberRole, string partnerRole)
```

**Parameters**

- `memberRole` (`string`)
- `partnerRole` (`string`)

### Fields

#### MemberRole

```csharp
public string MemberRole
```

**Type** `string`

#### PartnerRole

```csharp
public string PartnerRole
```

**Type** `string`

### Methods

#### ToString()

```csharp
public override string ToString()
```

**Returns** `string`

---

## SavedBeatsheet

*Class* — `StoryCADLib.Models.SavedBeatsheet`

Beatsheet model that is able to be saved/loaded from a file.

```csharp
public class SavedBeatsheet
```

### Constructors

#### SavedBeatsheet()

```csharp
public SavedBeatsheet()
```

### Properties

#### Description

Descritpion of beatsheet.

```csharp
[JsonInclude]
public string Description { get; set; }
```

**Type** `string`

#### Beats

Story beats

```csharp
[JsonInclude]
public List<StructureBeatViewModel> Beats { get; set; }
```

**Type** `List<StructureBeatViewModel>`

---

## SceneModel

*Class* — `StoryCADLib.Models.SceneModel`

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

*Class* — `StoryCADLib.Models.SettingModel`

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

*Class* — `StoryCADLib.Models.StoryDocument`

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

- `model` (`StoryModel`) — The story model (required)
- `filePath` (`string`) — The file path (optional, null for new documents)

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

*Class* — `StoryCADLib.Models.StoryElement`

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

- `name` (`string`) — Name of element
- `type` (`StoryItemType`) — Type of element
- `model` (`StoryModel`) — Story Model this element belongs to
- `parentNode` (`StoryNodeItem`) — Parent of this node

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

- `guid` (`Guid`) — The Guid of the StoryElement to retrieve
- `storyModel` (`StoryModel`) — optional story model override, defaults to current app state model.

**Returns** `StoryElement`

#### Deserialize(string)

Deserializes a JSON string into a StoryElement.

```csharp
public static StoryElement Deserialize(string json)
```

**Parameters**

- `json` (`string`) — JSON to deserialize.

**Returns** `StoryElement` — StoryElement Object.

#### Serialize()

Serialises this StoryElement into JSON.

```csharp
public string Serialize()
```

**Returns** `string` — JSON Representation of this object.

#### ToString()

```csharp
public override string ToString()
```

**Returns** `string`

---

## StoryElementCollection

*Class* — `StoryCADLib.Models.StoryElementCollection`

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

*Enum* — `StoryCADLib.Models.StoryItemType`

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

*Class* — `StoryCADLib.Models.StoryModel`

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

## StoryViewType

*Enum* — `StoryCADLib.Models.StoryViewType`

Views Modes available in StoryCAD UI.

```csharp
public enum StoryViewType
```

### Values

| Value | Description |
|-------|-------------|
| `ExplorerView` |  |
| `NarratorView` |  |

---

## StoryWorldModel

*Class* — `StoryCADLib.Models.StoryWorldModel`

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

## StringSelection

*Class* — `StoryCADLib.Models.StringSelection`

```csharp
public class StringSelection : ObservableObject
```

### Constructors

#### StringSelection(string, bool)

```csharp
public StringSelection(string stringName, bool selected = false)
```

**Parameters**

- `stringName` (`string`)
- `selected` (`bool`)

### Properties

#### StringName

```csharp
public string StringName { get; set; }
```

**Type** `string`

#### Selection

```csharp
public bool Selection { get; set; }
```

**Type** `bool`

---

## TrashCanModel

*Class* — `StoryCADLib.Models.TrashCanModel`

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

*Class* — `StoryCADLib.Models.WebModel`

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

---

## Windowing

*Class* — `StoryCADLib.Models.Windowing`

This class contains window (MainWindow) related items etc.

```csharp
public class Windowing : ObservableRecipient
```

### Constructors

#### Windowing(AppState, ILogService)

```csharp
public Windowing(AppState appState, ILogService logService)
```

**Parameters**

- `appState` (`AppState`)
- `logService` (`ILogService`)

### Fields

#### WindowHandle

A pointer to the App Window (MainWindow) handle

```csharp
public nint WindowHandle
```

**Type** `nint`

#### PageKey

```csharp
public string PageKey
```

**Type** `string`

#### MainWindow

```csharp
public Window MainWindow
```

**Type** `Window`

#### XamlRoot

A defect in early WinUI 3 Win32 code is that ContentDialog
controls don't have an established XamlRoot. A workaround
is to assign the dialog's XamlRoot to the root of a visible
Page. The Shell page's XamlRoot is stored here and accessed wherever needed.

```csharp
public XamlRoot XamlRoot
```

**Type** `XamlRoot`

#### GlobalDispatcher

A universal dispatcher to show messages/change UI from
a non UI thread. Example: Showing a warning from backup.

```csharp
public DispatcherQueue GlobalDispatcher
```

**Type** `DispatcherQueue`

### Properties

#### RequestedTheme

```csharp
public ElementTheme RequestedTheme { get; set; }
```

**Type** `ElementTheme`

#### AccentColor

Returns the users accent color
(Set in Windows Settings)

```csharp
public Color AccentColor { get; }
```

**Type** `Color`

#### PrimaryColor

Sets the shell color

```csharp
public SolidColorBrush PrimaryColor { get; set; }
```

**Type** `SolidColorBrush`

#### SecondaryColor

Handles various other colorations

```csharp
public SolidColorBrush SecondaryColor { get; set; }
```

**Type** `SolidColorBrush`

#### ContrastColor

This is a color that should in most cases
contrast the users accent color

```csharp
public SolidColorBrush ContrastColor { get; }
```

**Type** `SolidColorBrush`

### Methods

#### UpdateWindowTitle()

This will dynamically update the title based
on the current conditions of the app.

```csharp
public void UpdateWindowTitle()
```

#### UpdateUIToTheme()

This will update the elements of the UI to
match the theme set in RequestedTheme.

```csharp
public void UpdateUIToTheme()
```

#### ShowContentDialog(ContentDialog, bool)

This takes a ContentDialog and shows it to the user
It will handle theming, XAMLRoot and showing the dialog.

```csharp
public Task<ContentDialogResult> ShowContentDialog(ContentDialog Dialog, bool force = false)
```

**Parameters**

- `Dialog` (`ContentDialog`) — Content dialog to show
- `force` (`bool`) — Force show content dialog, will close currently open dialog if one
            is already open

**Returns** `Task<ContentDialogResult>` — A ContentDialogResult value

#### CloseContentDialog()

Dismisses the current content dialog

```csharp
public void CloseContentDialog()
```

#### ActivateMainInstance()

When a second instance is opened, this code will be ran on the main (first) instance
It will bring up the main window.

```csharp
public void ActivateMainInstance()
```

#### ShowFilePicker(string, string)

Shows a file picker.

```csharp
public Task<StorageFile> ShowFilePicker(string buttonText = "Open", string filter = "*")
```

**Parameters**

- `buttonText` (`string`)
- `filter` (`string`)

**Returns** `Task<StorageFile>` — A StorageFile object, of the file picked.

#### ShowFileSavePicker(string, string)

Shows a file save as picker.

```csharp
public Task<StorageFile> ShowFileSavePicker(string buttonText, string extension)
```

**Parameters**

- `buttonText` (`string`)
- `extension` (`string`)

**Returns** `Task<StorageFile>` — A StorageFile object, of the file picked.

#### ShowFolderPicker(string, string)

Spawn a folder picker for the user to select a folder.

```csharp
public Task<StorageFolder> ShowFolderPicker(string buttonText = "Select folder", string filter = "*")
```

**Parameters**

- `buttonText` (`string`) — Text shown on the confirmation button
- `filter` (`string`) — Filter filetype?

**Returns** `Task<StorageFolder>`

#### SetWindowSize(Window, double, double)

Sets the window size in physical pixels (consistent across all platforms regardless of DPI)

```csharp
public void SetWindowSize(Window window, double desiredWidthPx, double desiredHeightPx)
```

**Parameters**

- `window` (`Window`) — The Window to resize
- `desiredWidthPx` (`double`) — Desired width in physical pixels
- `desiredHeightPx` (`double`) — Desired height in physical pixels

#### CenterOnScreen(Window)

```csharp
public void CenterOnScreen(Window window)
```

**Parameters**

- `window` (`Window`)

#### GetActiveWindow()

```csharp
public static extern nint GetActiveWindow()
```

**Returns** `nint`

#### ShowResourceErrorMessage()

Shows an error message to the user that there's an issue
with the app, and it needs to be reinstalled.

```csharp
public void ShowResourceErrorMessage()
```

---

## Windowing.IInitializeWithWindow

*Interface* — `StoryCADLib.Models.Windowing.IInitializeWithWindow`

```csharp
[Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface Windowing.IInitializeWithWindow
```

### Methods

#### Initialize(nint)

```csharp
void Initialize(nint hwnd)
```

**Parameters**

- `hwnd` (`nint`)

---

## WorkflowStepModel

*Class* — `StoryCADLib.Models.WorkflowStepModel`

```csharp
public class WorkflowStepModel
```

### Constructors

#### WorkflowStepModel()

```csharp
public WorkflowStepModel()
```

### Properties

#### Name

```csharp
public string Name { get; set; }
```

**Type** `string`

#### Description

```csharp
public string Description { get; set; }
```

**Type** `string`

#### IsCompleted

```csharp
public bool IsCompleted { get; set; }
```

**Type** `bool`
