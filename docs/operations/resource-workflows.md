---
layout: default
title: "Resource Workflows"
parent: "Operations"
nav_order: 2
---

# Resource API Workflows

This page provides step-by-step code examples for each multi-step resource workflow. For an overview of what each resource provides, see [Resource Data and Writing Tools](../concepts/resource-data.md).

## Conflict Builder (4 Steps)

Build a structured conflict description for a Problem's protagonist or antagonist.

<div class="code-tabs" markdown="1">

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

```python
# Step 1: Get conflict categories
categories = sc.get_conflict_categories()
# ["Relationship", "Situational", "Inner Conflict", "Paranormal",
#  "Criminal activities", "Mystery and suspense", "Social drama", "Romantic"]

# Step 2: Get subcategories for a category
subcategories = sc.get_conflict_subcategories("Relationship")
# ["Lovers", "Family", "Friends", "Rivals", ...]

# Step 3: Get example conflicts for the subcategory
examples = sc.get_conflict_examples("Relationship", "Lovers")
# ["Jealousy threatens to destroy a passionate affair",
#  "A secret from the past resurfaces", ...]

# Step 4: Apply to a Problem element
problem = ...  # an existing Problem element handle

# Apply to protagonist's conflict
sc.apply_conflict_to_protagonist(problem, examples[0])

# Or apply to antagonist's conflict
sc.apply_conflict_to_antagonist(problem, "Custom conflict text is also valid")
```

</div>

### Presenting to Users or LLMs

For interactive applications, present each step as a choice:

<div class="code-tabs" markdown="1">

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

```python
# Show categories
print("Choose a conflict category:")
categories = sc.get_conflict_categories()
for i, category in enumerate(categories):
    print(f"  {i + 1}. {category}")

# After user selects, show subcategories
selected = categories[user_choice - 1]
subcategories = sc.get_conflict_subcategories(selected)
print(f"\nSubcategories for '{selected}':")
for i, subcategory in enumerate(subcategories):
    print(f"  {i + 1}. {subcategory}")

# After user selects subcategory, show examples
selected_sub = subcategories[user_choice - 1]
examples = sc.get_conflict_examples(selected, selected_sub)
print(f"\nExamples for '{selected}' > '{selected_sub}':")
for example in examples:
    print(f"  - {example}")
```

</div>

For an LLM agent, provide all categories in a single prompt and let it choose, or walk through the steps in a multi-turn conversation.

## Key Questions (2 Steps)

Retrieve development prompts organized by element type and topic.

<div class="code-tabs" markdown="1">

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

```python
# Step 1: Discover which element types have key questions
element_types = sc.get_key_question_elements()
# ["Character", "Problem", "Scene", "Setting", "Overview"]

# Step 2: Get questions for a specific type
questions = sc.get_key_questions("Character")
for q in questions:
    print(f"[{q.topic}] {q.question}")
# [Motivation] What does this character want more than anything?
# [Backstory] What event from their past shaped who they are today?
# [Relationships] Who is the most important person in their life?
# ...
```

</div>

### Using Key Questions for Element Development

Key questions are ideal for guiding an author (or LLM) through element creation:

<div class="code-tabs" markdown="1">

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

```python
# Create a character, then use key questions to flesh it out
character = sc.add_element(sc.item_type.Character, overview, "Detective Morgan")

questions = sc.get_key_questions("Character")
for q in questions:
    # Present question to user or LLM
    print(f"\n{q.topic}: {q.question}")
    answer = get_user_input()  # or LLM response

    # Map topic to property (your application logic)
    # e.g., "Motivation" → ProtGoal, "Backstory" → BackStory
    if q.topic == "Backstory" and answer:
        sc.update_element_property(character, "BackStory", answer)
```

</div>

## Master Plots (3 Steps)

Explore Tobias's 20 Master Plots to find a structure for your story.

<div class="code-tabs" markdown="1">

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

```python
# Step 1: List all master plots
plot_names = sc.get_master_plot_names()
# ["Quest", "Adventure", "Pursuit", "Rescue", "Escape", "Revenge", ...]

# Step 2: Read the notes for a plot
notes = sc.get_master_plot_notes("Quest")
print(notes)
# "The Quest plot sends the protagonist on a journey to find something..."

# Step 3: Get the scene breakdown
scenes = sc.get_master_plot_scenes("Quest")
for scene in scenes:
    print(f"\n{scene.title}")
    print(f"  {scene.notes}")
# Call to Adventure
#   The hero receives a call to leave their ordinary world...
# Crossing the Threshold
#   The hero commits to the journey and enters the special world...
# ...
```

</div>

### Creating Scenes from a Master Plot

Use the scene breakdown to populate an outline:

<div class="code-tabs" markdown="1">

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

```python
plot_scenes = sc.get_master_plot_scenes("Quest")
overview = ...  # the StoryOverview element handle

for plot_scene in plot_scenes:
    # add_element raises on failure instead of returning a result object
    scene = sc.add_element(sc.item_type.Scene, overview, plot_scene.title)
    sc.update_element_property(scene, "Notes", plot_scene.notes)
```

</div>

## Stock Scenes (2 Steps)

Browse scene templates organized by genre or purpose.

<div class="code-tabs" markdown="1">

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

```python
# Step 1: List categories
categories = sc.get_stock_scene_categories()
# ["Action", "Romance", "Mystery", "Horror", ...]

# Step 2: Get scenes for a category
scenes = sc.get_stock_scenes("Mystery")
for scene in scenes:
    print(f"  - {scene}")
# - The detective discovers a vital clue at the crime scene
# - A witness reveals a contradictory account
# - The suspect's alibi falls apart under questioning
# ...
```

</div>

### Creating a Scene from Stock

<div class="code-tabs" markdown="1">

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

```python
stock_scenes = sc.get_stock_scenes("Mystery")
selected_scene = stock_scenes[0]  # or user's choice

# add_element raises on failure instead of returning a result object
scene = sc.add_element(sc.item_type.Scene, overview, "Crime Scene Discovery")
sc.update_element_property(scene, "Notes", selected_scene)
```

</div>

## Combining Workflows

Resources are most powerful when combined. Here's an example that creates a Problem with conflict and structure:

<div class="code-tabs" markdown="1">

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

```python
# Create a problem
problem = sc.add_element(sc.item_type.Problem, overview, "The Missing Artifact")

# Use the Conflict Builder to set up protagonist conflict
examples = sc.get_conflict_examples("Mystery and suspense", "Whodunit")
sc.apply_conflict_to_protagonist(problem, examples[0])

# Apply a beat sheet for structure
sc.apply_beat_sheet_to_problem(problem, "Three Act Structure")

# Use master plot scenes to inspire the beat assignments
plot_scenes = sc.get_master_plot_scenes("Quest")
# Create scenes and assign them to beats — see Beat Sheet Operations
```

</div>

See [Beat Sheet Operations](beat-sheets.md) for the complete beat sheet workflow.
