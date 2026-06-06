---
layout: default
title: "Beat Sheet Operations"
parent: "Operations"
nav_order: 3
---

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

<div class="code-tabs" markdown="1">

```csharp
var names = api.GetBeatSheetNames().Payload;
foreach (var name in names)
    Console.WriteLine(name);
// Three Act Structure
// Hero's Journey
// Save the Cat
// ...
```

```python
names = sc.get_beat_sheet_names()
for name in names:
    print(name)
# Three Act Structure
# Hero's Journey
# Save the Cat
# ...
```

</div>

## Step 2: Preview a Template

<div class="code-tabs" markdown="1">

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

```python
sheet = sc.get_beat_sheet("Three Act Structure")

for beat in sheet.beats:
    print(f"\n  {beat.title}")
    print(f"    {beat.description}")
# Setup
#   Introduce the protagonist, setting, and status quo...
# Inciting Incident
#   An event disrupts the protagonist's world...
# ...
```

</div>

## Step 3: Apply to a Problem

<div class="code-tabs" markdown="1">

```csharp
var problemGuid = /* GUID of an existing Problem element */;
var result = api.ApplyBeatSheetToProblem(problemGuid, "Three Act Structure");

if (result.IsSuccess)
    Console.WriteLine("Beat sheet applied!");
```

```python
problem = ...  # handle of an existing Problem element
# The wrapper raises on failure instead of returning a success flag.
sc.apply_beat_sheet_to_problem(problem, "Three Act Structure")
print("Beat sheet applied!")
```

</div>

This replaces any existing beats on the Problem. The Problem's `StructureTitle` and `StructureDescription` are set from the template, and `StructureBeats` is populated with the template's beats.

## Step 4: View the Structure

<div class="code-tabs" markdown="1">

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

```python
structure = sc.get_problem_structure(problem)

for index, beat in enumerate(structure.beats):
    print(f"  [{index}] {beat.title}")
    print(f"      {beat.description}")
```

</div>

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

<div class="code-tabs" markdown="1">

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

```python
# Create scenes for the beats
scene1 = sc.add_element(sc.item_type.Scene, overview, "Opening Scene")
scene2 = sc.add_element(sc.item_type.Scene, overview, "The Discovery")

# Assign scenes to beats
sc.assign_element_to_beat(problem, 0, scene1)  # Beat 0: "Setup"
sc.assign_element_to_beat(problem, 1, scene2)  # Beat 1: "Inciting Incident"

# Verify
updated = sc.get_problem_structure(problem)
for beat in updated.beats:
    print(f"  {beat.title}")
```

</div>

### Finding Elements to Assign

To find existing Scenes or Problems to assign to beats:

<div class="code-tabs" markdown="1">

```csharp
// Option 1: Get all scenes
var scenes = api.GetElementsByType(StoryItemType.Scene).Payload;

// Option 2: Search by name
var results = api.SearchForText("confrontation").Payload;
var sceneResults = results.Where(r => r["Type"].ToString() == "Scene");
```

```python
# Option 1: Get all scenes
scenes = sc.get_elements_by_type(sc.item_type.Scene)

# Option 2: Search by name
results = sc.search_for_text("confrontation")
scene_results = [r for r in results if r["Type"] == "Scene"]
```

</div>

### Clearing an Assignment

<div class="code-tabs" markdown="1">

```csharp
api.ClearBeatAssignment(problemGuid, 1);  // Clears beat at index 1
```

```python
sc.clear_beat_assignment(problem, 1)  # Clears beat at index 1
```

</div>

The beat remains in the structure but has no linked element.

## Customizing Beats

### Create a New Beat

Adds a beat at the end of the list.

<div class="code-tabs" markdown="1">

```csharp
api.CreateBeat(problemGuid, "Epilogue", "The aftermath of the resolution");
```

```python
sc.create_beat(problem, "Epilogue", "The aftermath of the resolution")
```

</div>

### Update a Beat

Changes the title and/or description. Preserves any existing element assignment.

<div class="code-tabs" markdown="1">

```csharp
api.UpdateBeat(problemGuid, 0, "Opening Hook", "Grab the reader with an immediate conflict");
```

```python
sc.update_beat(problem, 0, "Opening Hook", "Grab the reader with an immediate conflict")
```

</div>

### Delete a Beat

Removes a beat and its assignment. Remaining beats shift down.

<div class="code-tabs" markdown="1">

```csharp
api.DeleteBeat(problemGuid, 5);  // Remove beat at index 5
```

```python
sc.delete_beat(problem, 5)  # Remove beat at index 5
```

</div>

### Move a Beat

Reorders a beat from one position to another.

<div class="code-tabs" markdown="1">

```csharp
api.MoveBeat(problemGuid, 3, 1);  // Move beat from index 3 to index 1
```

```python
sc.move_beat(problem, 3, 1)  # Move beat from index 3 to index 1
```

</div>

## Saving and Loading Custom Beat Sheets

Custom beat sheets can be saved to `.stbeat` files and loaded into other Problems.

### Save

<div class="code-tabs" markdown="1">

```csharp
// Save the Problem's current beat structure to a file
api.SaveBeatSheet(problemGuid, "my-structure.stbeat");
```

```python
# Save the Problem's current beat structure to a file
# (no documented Python equivalent for save_beat_sheet — see the C# tab)
sc.save_beat_sheet(problem, "my-structure.stbeat")
```

</div>

The Problem must have beats (from `ApplyBeatSheetToProblem` or `CreateBeat`).

### Load

<div class="code-tabs" markdown="1">

```csharp
// Load a .stbeat file into a different Problem
var otherProblemGuid = /* another Problem's GUID */;
api.LoadBeatSheet(otherProblemGuid, "my-structure.stbeat");
```

```python
# Load a .stbeat file into a different Problem
other_problem = ...  # handle of another Problem
# (no documented Python equivalent for load_beat_sheet — see the C# tab)
sc.load_beat_sheet(other_problem, "my-structure.stbeat")
```

</div>

Loading replaces any existing beats on the target Problem. The `StructureTitle` is set to "Custom Beat Sheet".

## Complete Example

Build a full story structure from a beat sheet template:

<div class="code-tabs" markdown="1">

```csharp
// Initialize
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

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

```python
# Initialize
sc = StoryCAD(headless=True)

# Create outline
overview = sc.create_empty_outline("Mystery Novel", "Author", template_index="0")[0]

# Create the main problem
problem = sc.add_element(sc.item_type.Problem, overview, "Who Killed Mr. Gray?")

# Apply a beat sheet
sc.apply_beat_sheet_to_problem(problem, "Three Act Structure")

# View the beats
structure = sc.get_problem_structure(problem)

# Create a scene for each beat and assign it
# (the wrapper raises on failure, so no success check is needed)
for beat_index, beat in enumerate(structure.beats):
    # Create a scene named after the beat
    scene = sc.add_element(sc.item_type.Scene, overview, beat.title)

    # Set the scene's notes from the beat description
    sc.update_element_property(scene, "Notes", beat.description)

    # Assign the scene to the beat
    sc.assign_element_to_beat(problem, beat_index, scene)

# Add a custom epilogue beat
sc.create_beat(problem, "Denouement", "The detective reveals the truth")
epilogue = sc.add_element(sc.item_type.Scene, overview, "The Reveal")
sc.assign_element_to_beat(problem, len(structure.beats), epilogue)

# Save
sc.write_outline("mystery-novel.stbx")
```

</div>

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
