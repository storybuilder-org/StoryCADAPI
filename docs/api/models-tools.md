---
layout: default
title: Models.Tools
parent: API Reference
nav_order: 4
---

# Models.Tools

Auto-generated reference for the `StoryCADLib.Models.Tools` namespace (10 types).

{: .fs-6 .fw-300 }

## ConflictCategoryModel

*Class* — `StoryCADLib.Models.Tools.ConflictCategoryModel`

```csharp
public class ConflictCategoryModel
```

### Constructors

#### ConflictCategoryModel(string)

```csharp
public ConflictCategoryModel(string topic)
```

**Parameters**

- `topic` (`string`)

### Fields

#### Examples

```csharp
public SortedDictionary<string, List<string>> Examples
```

**Type** `SortedDictionary<string, List<string>>`

#### SubCategories

```csharp
public List<string> SubCategories
```

**Type** `List<string>`

#### TopicName

```csharp
public string TopicName
```

**Type** `string`

---

## DramaticSituationModel

*Class* — `StoryCADLib.Models.Tools.DramaticSituationModel`

```csharp
public class DramaticSituationModel
```

### Constructors

#### DramaticSituationModel(string)

```csharp
public DramaticSituationModel(string situationName)
```

**Parameters**

- `situationName` (`string`)

### Properties

#### SituationName

```csharp
public string SituationName { get; set; }
```

**Type** `string`

#### Role1

```csharp
public string Role1 { get; set; }
```

**Type** `string`

#### Role2

```csharp
public string Role2 { get; set; }
```

**Type** `string`

#### Role3

```csharp
public string Role3 { get; set; }
```

**Type** `string`

#### Role4

```csharp
public string Role4 { get; set; }
```

**Type** `string`

#### Description1

```csharp
public string Description1 { get; set; }
```

**Type** `string`

#### Description2

```csharp
public string Description2 { get; set; }
```

**Type** `string`

#### Description3

```csharp
public string Description3 { get; set; }
```

**Type** `string`

#### Description4

```csharp
public string Description4 { get; set; }
```

**Type** `string`

#### Notes

```csharp
public string Notes { get; set; }
```

**Type** `string`

---

## KeyQuestionModel

*Class* — `StoryCADLib.Models.Tools.KeyQuestionModel`

```csharp
public class KeyQuestionModel : ObservableObject
```

### Constructors

#### KeyQuestionModel()

```csharp
public KeyQuestionModel()
```

### Properties

#### Key

```csharp
public string Key { get; set; }
```

**Type** `string`

#### Element

```csharp
public string Element { get; set; }
```

**Type** `string`

#### Topic

```csharp
public string Topic { get; set; }
```

**Type** `string`

#### Question

```csharp
public string Question { get; set; }
```

**Type** `string`

---

## PlotPatternModel

*Class* — `StoryCADLib.Models.Tools.PlotPatternModel`

```csharp
public class PlotPatternModel
```

### Constructors

#### PlotPatternModel(string)

```csharp
public PlotPatternModel(string name)
```

**Parameters**

- `name` (`string`)

### Fields

#### PlotPatternName

```csharp
public string PlotPatternName
```

**Type** `string`

#### PlotPatternNotes

```csharp
public string PlotPatternNotes
```

**Type** `string`

#### PlotPatternScenes

```csharp
public List<PlotPatternScene> PlotPatternScenes
```

**Type** `List<PlotPatternScene>`

---

## PlotPatternScene

*Class* — `StoryCADLib.Models.Tools.PlotPatternScene`

```csharp
public class PlotPatternScene
```

### Constructors

#### PlotPatternScene(string)

```csharp
public PlotPatternScene(string title)
```

**Parameters**

- `title` (`string`)

### Fields

#### Notes

```csharp
public string Notes
```

**Type** `string`

#### SceneTitle

```csharp
public string SceneTitle
```

**Type** `string`

---

## PreferencesModel

*Class* — `StoryCADLib.Models.Tools.PreferencesModel`

PreferencesModel contains StoryCAD User preferences.
The model is maintained from a Shell Preferences() method
which is launched from a Command tied to a View button, and by
PreferencesViewModel using a ContentDialog as the view.
The StoryCAD user preferences are stored as Preferences.json within AppState.RootDirectory.
If Preferences.json doesn't exist, it will be created once the user hits done within
the preferences initialisation screen.

```csharp
public class PreferencesModel : ObservableObject
```

### Constructors

#### PreferencesModel()

```csharp
public PreferencesModel()
```

### Properties

#### FirstName

This is the user's first name

```csharp
[JsonInclude]
[JsonPropertyName("FirstName")]
public string FirstName { get; set; }
```

**Type** `string`

#### LastName

This is the user's surname

```csharp
[JsonInclude]
[JsonPropertyName("LastName")]
public string LastName { get; set; }
```

**Type** `string`

#### Email

This is the user's email
(Used to get in contact for errors and newsletter if enabled)

```csharp
[JsonInclude]
[JsonPropertyName("Email")]
public string Email { get; set; }
```

**Type** `string`

#### ErrorCollectionConsent

Disables Elmah.io integration if false

```csharp
[JsonInclude]
[JsonPropertyName("ElmahConsent")]
public bool ErrorCollectionConsent { get; set; }
```

**Type** `bool`

#### Newsletter

If set to true user email will be added to the newsletter list

```csharp
[JsonInclude]
[JsonPropertyName("NewsletterConsent")]
public bool Newsletter { get; set; }
```

**Type** `bool`

#### PreferencesInitialized

This switch tracks whether this is the first time StoryCAD is opened

```csharp
[JsonInclude]
[JsonPropertyName("Initialized")]
public bool PreferencesInitialized { get; set; }
```

**Type** `bool`

#### LastSelectedTemplate

Tracks the last used template, for new outline creation

```csharp
[JsonInclude]
[JsonPropertyName("LastTemplate")]
public int LastSelectedTemplate { get; set; }
```

**Type** `int`

#### ThemePreference

This is the user's theme preference

```csharp
[JsonInclude]
[JsonPropertyName("Theme")]
public ElementTheme ThemePreference { get; set; }
```

**Type** `ElementTheme`

#### WrapNodeNames

If set to wrap, node names will wrap in the tree.
If set to disabled, node names will cut off.

```csharp
[JsonInclude]
[JsonPropertyName("NodeWrap")]
public TextWrapping WrapNodeNames { get; set; }
```

**Type** `TextWrapping`

#### AutoSave

StoryCAD will automatically save the outline if true

```csharp
[JsonInclude]
[JsonPropertyName("Autosave")]
public bool AutoSave { get; set; }
```

**Type** `bool`

#### AutoSaveInterval

Controls how often autosave is run
(Ignored if AutoSave is off)

```csharp
[JsonInclude]
[JsonPropertyName("AutosaveInterval")]
public int AutoSaveInterval { get; set; }
```

**Type** `int`

#### BackupOnOpen

StoryCAD will create a backup of the outline when opened if true

```csharp
[JsonInclude]
[JsonPropertyName("BackupOnOpen")]
public bool BackupOnOpen { get; set; }
```

**Type** `bool`

#### TimedBackup

StoryCAD will create a backup of the currently opened outline if true.

```csharp
[JsonInclude]
[JsonPropertyName("TimedBackup")]
public bool TimedBackup { get; set; }
```

**Type** `bool`

#### TimedBackupInterval

Controls timed backup frequency.

```csharp
[JsonInclude]
[JsonPropertyName("TimedBackupInterval")]
public int TimedBackupInterval { get; set; }
```

**Type** `int`

#### ProjectDirectory

Default location where outlines are stored.

```csharp
[JsonInclude]
[JsonPropertyName("OutlineDirectory")]
public string ProjectDirectory { get; set; }
```

**Type** `string`

#### BackupDirectory

Location where backups are stored if enabled

```csharp
[JsonInclude]
[JsonPropertyName("BackupDirectory")]
public string BackupDirectory { get; set; }
```

**Type** `string`

#### RecentFiles

Recently opened files
(Capped at 25)

```csharp
[JsonInclude]
[JsonPropertyName("RecentFiles")]
public List<string> RecentFiles { get; set; }
```

**Type** `List<string>`

#### Version

Tracks last version of StoryCAD that was opened

```csharp
[JsonInclude]
[JsonPropertyName("Version")]
public string Version { get; set; }
```

**Type** `string`

#### RecordPreferencesStatus

```csharp
[JsonInclude]
[JsonPropertyName("RecordPreferencesStatus")]
public bool RecordPreferencesStatus { get; set; }
```

**Type** `bool`

#### RecordVersionStatus

```csharp
[JsonInclude]
[JsonPropertyName("RecordVersionStatus")]
public bool RecordVersionStatus { get; set; }
```

**Type** `bool`

#### PreferredSearchEngine

```csharp
[JsonInclude]
[JsonPropertyName("PreferredSearchEngine")]
public BrowserType PreferredSearchEngine { get; set; }
```

**Type** `BrowserType`

#### SearchEngineIndex

Search engine to use.

```csharp
[JsonInclude]
[JsonPropertyName("SearchEngineIndex")]
public int SearchEngineIndex { get; set; }
```

**Type** `int`

#### HideRatingPrompt

Hides the rating prompt until the next update

```csharp
[JsonInclude]
[JsonPropertyName("ShowRatings")]
public bool HideRatingPrompt { get; set; }
```

**Type** `bool`

#### CumulativeTimeUsed

Total amount of time StoryCAD has been used/open on the system

```csharp
[JsonInclude]
[JsonPropertyName("TimeUsed")]
public long CumulativeTimeUsed { get; set; }
```

**Type** `long`

#### LastReviewDate

DateTime of last review

```csharp
[JsonInclude]
[JsonPropertyName("LastReview")]
public DateTime LastReviewDate { get; set; }
```

**Type** `DateTime`

#### ShowStartupDialog

Should the startup dialog (HelpPage) be shown

```csharp
[JsonInclude]
[JsonPropertyName("ShowStartupDialog")]
public bool ShowStartupDialog { get; set; }
```

**Type** `bool`

#### AdvancedLogging

Do we want to log more in depth

```csharp
[JsonInclude]
[JsonPropertyName("AdvancedLogging")]
public bool AdvancedLogging { get; set; }
```

**Type** `bool`

#### HideKeyFileWarning

Hide the key file missing warning dialog.

```csharp
[JsonInclude]
[JsonPropertyName("HideKeyFileWarning")]
public bool HideKeyFileWarning { get; set; }
```

**Type** `bool`

#### ShowFilePickerOnStartup

Should the file picker be shown on startup

```csharp
[JsonInclude]
[JsonPropertyName("ShowFilePickerOnStartup")]
public bool ShowFilePickerOnStartup { get; set; }
```

**Type** `bool`

---

## SubTopicModel

*Class* — `StoryCADLib.Models.Tools.SubTopicModel`

```csharp
public class SubTopicModel
```

### Constructors

#### SubTopicModel(string)

```csharp
public SubTopicModel(string name)
```

**Parameters**

- `name` (`string`)

### Fields

#### SubTopicName

```csharp
public string SubTopicName
```

**Type** `string`

#### SubTopicNotes

```csharp
public string SubTopicNotes
```

**Type** `string`

---

## ToolsData

*Class* — `StoryCADLib.Models.Tools.ToolsData`

This stores the tools for StoryCAD's Tools.json.
Previously tools were stored in GlobalData.

```csharp
public class ToolsData
```

### Constructors

#### ToolsData(ILogService, JSONResourceLoader)

```csharp
public ToolsData(ILogService log, JSONResourceLoader resourceLoader)
```

**Parameters**

- `log` (`ILogService`)
- `resourceLoader` (`JSONResourceLoader`)

### Fields

#### BeatSheetSource

```csharp
public List<PlotPatternModel> BeatSheetSource
```

**Type** `List<PlotPatternModel>`

#### DramaticSituationsSource

```csharp
public SortedDictionary<string, DramaticSituationModel> DramaticSituationsSource
```

**Type** `SortedDictionary<string, DramaticSituationModel>`

#### FemaleFirstNamesSource

```csharp
public ObservableCollection<string> FemaleFirstNamesSource
```

**Type** `ObservableCollection<string>`

#### KeyQuestionsSource

```csharp
public Dictionary<string, List<KeyQuestionModel>> KeyQuestionsSource
```

**Type** `Dictionary<string, List<KeyQuestionModel>>`

#### LastNamesSource

```csharp
public ObservableCollection<string> LastNamesSource
```

**Type** `ObservableCollection<string>`

#### MaleFirstNamesSource

```csharp
public ObservableCollection<string> MaleFirstNamesSource
```

**Type** `ObservableCollection<string>`

#### MasterPlotsSource

```csharp
public List<PlotPatternModel> MasterPlotsSource
```

**Type** `List<PlotPatternModel>`

#### RelationshipsSource

```csharp
public ObservableCollection<string> RelationshipsSource
```

**Type** `ObservableCollection<string>`

#### StockScenesSource

```csharp
public SortedDictionary<string, ObservableCollection<string>> StockScenesSource
```

**Type** `SortedDictionary<string, ObservableCollection<string>>`

#### TopicsSource

```csharp
public SortedDictionary<string, TopicModel> TopicsSource
```

**Type** `SortedDictionary<string, TopicModel>`

---

## TopicModel

*Class* — `StoryCADLib.Models.Tools.TopicModel`

```csharp
public class TopicModel
```

### Constructors

#### TopicModel(string, string)

```csharp
public TopicModel(string topic, string filename)
```

**Parameters**

- `topic` (`string`)
- `filename` (`string`)

#### TopicModel(string)

```csharp
public TopicModel(string topic)
```

**Parameters**

- `topic` (`string`)

### Fields

#### Filename

```csharp
public string Filename
```

**Type** `string`

#### SubTopics

```csharp
public List<SubTopicModel> SubTopics
```

**Type** `List<SubTopicModel>`

#### TopicName

```csharp
public string TopicName
```

**Type** `string`

#### TopicType

```csharp
public TopicTypeEnum TopicType
```

**Type** `TopicTypeEnum`

---

## TopicTypeEnum

*Enum* — `StoryCADLib.Models.Tools.TopicTypeEnum`

```csharp
public enum TopicTypeEnum
```

### Values

| Value | Description |
|-------|-------------|
| `Notepad` |  |
| `Inline` |  |
