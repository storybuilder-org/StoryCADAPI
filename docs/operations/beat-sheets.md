# Beat Sheet Operations

Beat sheets define a story's structure as a sequence of narrative beats. Each beat has a title, description, and an optional link to a Scene or Problem element. Beat sheets are applied to Problem elements — a Problem's structure tab holds the beat sheet data.

## Workflow Overview

```
GetBeatSheetNames          → list available templates
GetBeatSheet               → preview a template's beats
ApplyBeatSheetToProblem    → apply template to a Problem
GetProblemStructure        → view the Problem's current beats
AssignElementToBeat        → link a Scene/Problem to a beat
CreateBeat / UpdateBeat / DeleteBeat / MoveBeat → customize
SaveBeatSheet / LoadBeatSheet → persist custom beat sheets
```

## Step 1: Browse Templates

```csharp
var names = api.GetBeatSheetNames().Payload;
foreach (var name in names)
    Console.WriteLine(name);
// Three Act Structure
// Hero's Journey
// Save the Cat
// ...
```

## Step 2: Preview a Template

```csharp
var result = api.GetBeatSheet("Three Act Structure");
var (description, beats) = result.Payload;

Console.WriteLine(description);
// "The three-act structure divides a story into setup, confrontation, and resolution..."

foreach (var (beatName, beatNotes) in beats)
{
    Console.WriteLine($"\n  {beatName}");
    Console.WriteLine($"    {beatNotes}");
}
// Setup
//   Introduce the protagonist, setting, and status quo...
// Inciting Incident
//   An event disrupts the protagonist's world...
// ...
```

## Step 3: Apply to a Problem

```csharp
var problemGuid = /* GUID of an existing Problem element */;
var result = api.ApplyBeatSheetToProblem(problemGuid, "Three Act Structure");

if (result.IsSuccess)
    Console.WriteLine("Beat sheet applied!");
```

This replaces any existing beats on the Problem. The Problem's `StructureTitle` and `StructureDescription` are set from the template, and `StructureBeats` is populated with the template's beats.

## Step 4: View the Structure

```csharp
var structure = api.GetProblemStructure(problemGuid).Payload;
var (title, desc, beats) = structure;

Console.WriteLine($"Structure: {title}");
Console.WriteLine($"Description: {desc}\n");

int index = 0;
foreach (var (beatTitle, beatDesc, linkedElement) in beats)
{
    var assignment = linkedElement.HasValue ? $" → {linkedElement}" : " (unassigned)";
    Console.WriteLine($"  [{index}] {beatTitle}{assignment}");
    Console.WriteLine($"      {beatDesc}");
    index++;
}
```

**Output**:
```
Structure: Three Act Structure
Description: The three-act structure divides a story into...

  [0] Setup (unassigned)
      Introduce the protagonist, setting, and status quo...
  [1] Inciting Incident (unassigned)
      An event disrupts the protagonist's world...
  [2] First Plot Point (unassigned)
      ...
```

## Step 5: Assign Elements to Beats

Link Scene or Problem elements to beats by their 0-based index.

```csharp
// Create scenes for the beats
var scene1 = api.AddElement(StoryItemType.Scene, overviewGuid, "Opening Scene").Payload;
var scene2 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Discovery").Payload;

// Assign scenes to beats
api.AssignElementToBeat(problemGuid, 0, scene1);  // Beat 0: "Setup"
api.AssignElementToBeat(problemGuid, 1, scene2);  // Beat 1: "Inciting Incident"

// Verify
var updated = api.GetProblemStructure(problemGuid).Payload;
foreach (var (beatTitle, _, linked) in updated.Beats)
{
    if (linked.HasValue)
        Console.WriteLine($"  {beatTitle} → {linked}");
    else
        Console.WriteLine($"  {beatTitle} (unassigned)");
}
```

### Finding Elements to Assign

To find existing Scenes or Problems to assign to beats:

```csharp
// Option 1: Get all scenes
var scenes = api.GetElementsByType(StoryItemType.Scene).Payload;

// Option 2: Search by name
var results = api.SearchForText("confrontation").Payload;
var sceneResults = results.Where(r => r["Type"].ToString() == "Scene");
```

### Clearing an Assignment

```csharp
api.ClearBeatAssignment(problemGuid, 1);  // Clears beat at index 1
```

The beat remains in the structure but has no linked element.

## Customizing Beats

### Create a New Beat

Adds a beat at the end of the list.

```csharp
api.CreateBeat(problemGuid, "Epilogue", "The aftermath of the resolution");
```

### Update a Beat

Changes the title and/or description. Preserves any existing element assignment.

```csharp
api.UpdateBeat(problemGuid, 0, "Opening Hook", "Grab the reader with an immediate conflict");
```

### Delete a Beat

Removes a beat and its assignment. Remaining beats shift down.

```csharp
api.DeleteBeat(problemGuid, 5);  // Remove beat at index 5
```

### Move a Beat

Reorders a beat from one position to another.

```csharp
api.MoveBeat(problemGuid, 3, 1);  // Move beat from index 3 to index 1
```

## Saving and Loading Custom Beat Sheets

Custom beat sheets can be saved to `.stbeat` files and loaded into other Problems.

### Save

```csharp
// Save the Problem's current beat structure to a file
api.SaveBeatSheet(problemGuid, "my-structure.stbeat");
```

The Problem must have beats (from `ApplyBeatSheetToProblem` or `CreateBeat`).

### Load

```csharp
// Load a .stbeat file into a different Problem
var otherProblemGuid = /* another Problem's GUID */;
api.LoadBeatSheet(otherProblemGuid, "my-structure.stbeat");
```

Loading replaces any existing beats on the target Problem. The `StructureTitle` is set to "Custom Beat Sheet".

## Complete Example

Build a full story structure from a beat sheet template:

```csharp
// Initialize
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();

// Create outline
var outline = await api.CreateEmptyOutline("Mystery Novel", "Author", "0");
var overviewGuid = outline.Payload[0].ToString();

// Create the main problem
var problem = api.AddElement(StoryItemType.Problem, overviewGuid, "Who Killed Mr. Gray?");
var problemGuid = problem.Payload;

// Apply a beat sheet
api.ApplyBeatSheetToProblem(problemGuid, "Three Act Structure");

// View the beats
var structure = api.GetProblemStructure(problemGuid).Payload;

// Create a scene for each beat and assign it
int beatIndex = 0;
foreach (var (beatTitle, beatDesc, _) in structure.Beats)
{
    // Create a scene named after the beat
    var scene = api.AddElement(StoryItemType.Scene, overviewGuid, beatTitle);
    if (scene.IsSuccess)
    {
        // Set the scene's notes from the beat description
        api.UpdateElementProperty(scene.Payload, "Notes", beatDesc);

        // Assign the scene to the beat
        api.AssignElementToBeat(problemGuid, beatIndex, scene.Payload);
    }
    beatIndex++;
}

// Add a custom epilogue beat
api.CreateBeat(problemGuid, "Denouement", "The detective reveals the truth");
var epilogue = api.AddElement(StoryItemType.Scene, overviewGuid, "The Reveal");
if (epilogue.IsSuccess)
{
    api.AssignElementToBeat(problemGuid, beatIndex, epilogue.Payload);
}

// Save
await api.WriteOutline("mystery-novel.stbx");
```

## Method Reference

| Method | Parameters | Returns | Modifies? |
|--------|-----------|---------|-----------|
| `GetBeatSheetNames()` | — | `IEnumerable<string>` | No |
| `GetBeatSheet(name)` | Template name | `(Description, Beats)` | No |
| `ApplyBeatSheetToProblem(guid, name)` | Problem GUID, template name | `bool` | **Yes** |
| `GetProblemStructure(guid)` | Problem GUID | `(Title, Description, Beats)` | No |
| `AssignElementToBeat(guid, index, elementGuid)` | Problem GUID, beat index, element GUID | `bool` | **Yes** |
| `ClearBeatAssignment(guid, index)` | Problem GUID, beat index | `bool` | **Yes** |
| `CreateBeat(guid, title, desc)` | Problem GUID, title, description | `bool` | **Yes** |
| `UpdateBeat(guid, index, title, desc)` | Problem GUID, beat index, title, description | `bool` | **Yes** |
| `DeleteBeat(guid, index)` | Problem GUID, beat index | `bool` | **Yes** |
| `MoveBeat(guid, from, to)` | Problem GUID, from index, to index | `bool` | **Yes** |
| `SaveBeatSheet(guid, path)` | Problem GUID, file path | `bool` | No (writes file) |
| `LoadBeatSheet(guid, path)` | Problem GUID, file path | `bool` | **Yes** |
