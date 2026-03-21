# Resource API Workflows

This page provides step-by-step code examples for each multi-step resource workflow. For an overview of what each resource provides, see [Resource Data and Writing Tools](../concepts/resource-data.md).

## Conflict Builder (4 Steps)

Build a structured conflict description for a Problem's protagonist or antagonist.

```csharp
// Step 1: Get conflict categories
var categories = api.GetConflictCategories().Payload;
// ["Relationship", "Situational", "Inner Conflict", "Paranormal",
//  "Criminal activities", "Mystery and suspense", "Social drama", "Romantic"]

// Step 2: Get subcategories for a category
var subcategories = api.GetConflictSubcategories("Relationship").Payload;
// ["Lovers", "Family", "Friends", "Rivals", ...]

// Step 3: Get example conflicts for the subcategory
var examples = api.GetConflictExamples("Relationship", "Lovers").Payload;
// ["Jealousy threatens to destroy a passionate affair",
//  "A secret from the past resurfaces", ...]

// Step 4: Apply to a Problem element
var problemGuid = /* GUID of an existing Problem */;

// Apply to protagonist's conflict
api.ApplyConflictToProtagonist(problemGuid, examples.First());

// Or apply to antagonist's conflict
api.ApplyConflictToAntagonist(problemGuid, "Custom conflict text is also valid");
```

### Presenting to Users or LLMs

For interactive applications, present each step as a choice:

```csharp
// Show categories
Console.WriteLine("Choose a conflict category:");
var categories = api.GetConflictCategories().Payload.ToList();
for (int i = 0; i < categories.Count; i++)
    Console.WriteLine($"  {i + 1}. {categories[i]}");

// After user selects, show subcategories
var selected = categories[userChoice - 1];
var subcategories = api.GetConflictSubcategories(selected).Payload.ToList();
Console.WriteLine($"\nSubcategories for '{selected}':");
for (int i = 0; i < subcategories.Count; i++)
    Console.WriteLine($"  {i + 1}. {subcategories[i]}");

// After user selects subcategory, show examples
var selectedSub = subcategories[userChoice - 1];
var examples = api.GetConflictExamples(selected, selectedSub).Payload.ToList();
Console.WriteLine($"\nExamples for '{selected}' > '{selectedSub}':");
foreach (var example in examples)
    Console.WriteLine($"  - {example}");
```

For an LLM agent, provide all categories in a single prompt and let it choose, or walk through the steps in a multi-turn conversation.

## Key Questions (2 Steps)

Retrieve development prompts organized by element type and topic.

```csharp
// Step 1: Discover which element types have key questions
var elementTypes = api.GetKeyQuestionElements().Payload;
// ["Character", "Problem", "Scene", "Setting", "Overview"]

// Step 2: Get questions for a specific type
var questions = api.GetKeyQuestions("Character").Payload;
foreach (var (topic, question) in questions)
{
    Console.WriteLine($"[{topic}] {question}");
}
// [Motivation] What does this character want more than anything?
// [Backstory] What event from their past shaped who they are today?
// [Relationships] Who is the most important person in their life?
// ...
```

### Using Key Questions for Element Development

Key questions are ideal for guiding an author (or LLM) through element creation:

```csharp
// Create a character, then use key questions to flesh it out
var charResult = api.AddElement(StoryItemType.Character, overviewGuid, "Detective Morgan");
var charGuid = charResult.Payload;

var questions = api.GetKeyQuestions("Character").Payload;
foreach (var (topic, question) in questions)
{
    // Present question to user or LLM
    Console.WriteLine($"\n{topic}: {question}");
    var answer = GetUserInput(); // or LLM response

    // Map topic to property (your application logic)
    // e.g., "Motivation" → ProtGoal, "Backstory" → BackStory
    if (topic == "Backstory" && !string.IsNullOrEmpty(answer))
        api.UpdateElementProperty(charGuid, "BackStory", answer);
}
```

## Master Plots (3 Steps)

Explore Tobias's 20 Master Plots to find a structure for your story.

```csharp
// Step 1: List all master plots
var plotNames = api.GetMasterPlotNames().Payload;
// ["Quest", "Adventure", "Pursuit", "Rescue", "Escape", "Revenge", ...]

// Step 2: Read the notes for a plot
var notes = api.GetMasterPlotNotes("Quest").Payload;
Console.WriteLine(notes);
// "The Quest plot sends the protagonist on a journey to find something..."

// Step 3: Get the scene breakdown
var scenes = api.GetMasterPlotScenes("Quest").Payload;
foreach (var (sceneTitle, sceneNotes) in scenes)
{
    Console.WriteLine($"\n{sceneTitle}");
    Console.WriteLine($"  {sceneNotes}");
}
// Call to Adventure
//   The hero receives a call to leave their ordinary world...
// Crossing the Threshold
//   The hero commits to the journey and enters the special world...
// ...
```

### Creating Scenes from a Master Plot

Use the scene breakdown to populate an outline:

```csharp
var plotScenes = api.GetMasterPlotScenes("Quest").Payload;
var overviewGuid = /* GUID of the StoryOverview */;

foreach (var (title, notes) in plotScenes)
{
    var sceneResult = api.AddElement(
        StoryItemType.Scene,
        overviewGuid.ToString(),
        title);

    if (sceneResult.IsSuccess)
    {
        api.UpdateElementProperty(sceneResult.Payload, "Notes", notes);
    }
}
```

## Stock Scenes (2 Steps)

Browse scene templates organized by genre or purpose.

```csharp
// Step 1: List categories
var categories = api.GetStockSceneCategories().Payload;
// ["Action", "Romance", "Mystery", "Horror", ...]

// Step 2: Get scenes for a category
var scenes = api.GetStockScenes("Mystery").Payload;
foreach (var scene in scenes)
{
    Console.WriteLine($"  - {scene}");
}
// - The detective discovers a vital clue at the crime scene
// - A witness reveals a contradictory account
// - The suspect's alibi falls apart under questioning
// ...
```

### Creating a Scene from Stock

```csharp
var stockScenes = api.GetStockScenes("Mystery").Payload.ToList();
var selectedScene = stockScenes[0]; // or user's choice

var sceneResult = api.AddElement(
    StoryItemType.Scene,
    overviewGuid.ToString(),
    "Crime Scene Discovery");

if (sceneResult.IsSuccess)
{
    api.UpdateElementProperty(sceneResult.Payload, "Notes", selectedScene);
}
```

## Combining Workflows

Resources are most powerful when combined. Here's an example that creates a Problem with conflict and structure:

```csharp
// Create a problem
var problemResult = api.AddElement(
    StoryItemType.Problem,
    overviewGuid.ToString(),
    "The Missing Artifact");
var problemGuid = problemResult.Payload;

// Use the Conflict Builder to set up protagonist conflict
var examples = api.GetConflictExamples("Mystery and suspense", "Whodunit").Payload;
api.ApplyConflictToProtagonist(problemGuid, examples.First());

// Apply a beat sheet for structure
api.ApplyBeatSheetToProblem(problemGuid, "Three Act Structure");

// Use master plot scenes to inspire the beat assignments
var plotScenes = api.GetMasterPlotScenes("Quest").Payload.ToList();
// Create scenes and assign them to beats — see Beat Sheet Operations
```

See [Beat Sheet Operations](beat-sheets.md) for the complete beat sheet workflow.
