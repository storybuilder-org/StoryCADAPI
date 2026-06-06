---
layout: default
title: "Semantic Kernel Integration"
parent: "Advanced"
nav_order: 1
---

# Semantic Kernel Integration

The StoryCAD API is designed for use with [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/). Every public method on `StoryCADApi` is decorated with `[KernelFunction]` and `[Description]` attributes, making the API directly registerable as an SK plugin.

## How It Works

Semantic Kernel uses `[KernelFunction]` attributes to discover methods that an LLM can invoke via function calling. The `[Description]` attributes provide the LLM with documentation about what each method does, its parameters, and its constraints.

<div class="code-tabs" markdown="1">

```csharp
// From StoryCADAPI.cs — every method looks like this:
[KernelFunction]
[Description("Creates a new empty story outline from a template.")]
public async Task<OperationResult<List<Guid>>> CreateEmptyOutline(
    string name, string author, string templateIndex)
```

```python
# (no direct Python equivalent — [KernelFunction]/[Description] attributes and
# the OperationResult<T> return type are .NET / Semantic Kernel concepts.)
# In Python, the same method is just a plain call that returns the payload
# directly and raises on error:
from storycad import StoryCAD

sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("My Story", "Jake", template_index="0")[0]
```

</div>

When you register the API as a plugin, the LLM sees a catalog of 48+ functions it can call to create, query, and modify story outlines.

## Registering as a Plugin

<div class="code-tabs" markdown="1">

```csharp
using Microsoft.SemanticKernel;
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// 1. Initialize StoryCADLib
BootStrapper.Initialise(headless: true);

// 2. Get the API instance
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// 3. Build the Semantic Kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
var kernel = builder.Build();

// 4. Register the API as a plugin
kernel.ImportPluginFromObject(api, "StoryCAD");
```

```python
# 1 + 2. Initialize StoryCADLib and get the API instance (BootStrapper.Initialise
#        + Ioc.Default.GetRequiredService collapse into one call).
from storycad import StoryCAD

sc = StoryCAD(headless=True)

# 3 + 4. Building a Semantic Kernel and registering the API as an SK plugin have
#        no direct Python equivalent — Kernel.CreateBuilder, AddOpenAIChatCompletion,
#        and ImportPluginFromObject are .NET / Semantic Kernel concepts.
```

</div>

After step 4, the kernel exposes all `[KernelFunction]` methods as callable functions. The LLM can invoke them through function calling.

## Enabling Automatic Function Calling

For the LLM to call API methods autonomously, enable automatic function calling in the execution settings:

<div class="code-tabs" markdown="1">

```csharp
using Microsoft.SemanticKernel.Connectors.OpenAI;

var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
```

```python
# (no direct Python equivalent — OpenAIPromptExecutionSettings and
# FunctionChoiceBehavior.Auto() are .NET / Semantic Kernel concepts.)
```

</div>

With `Auto` behavior, the LLM decides when to call functions based on the conversation context. It sees the `[Description]` text for each method and chooses which to invoke.

## Building an Agent

Combine the plugin registration with a system prompt to create an agent that builds outlines through natural language:

<div class="code-tabs" markdown="1">

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Build kernel with plugin
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4o",
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
var kernel = builder.Build();
kernel.ImportPluginFromObject(api, "StoryCAD");

// Chat service and settings
var chat = kernel.GetRequiredService<IChatCompletionService>();
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// System prompt
var history = new ChatHistory();
history.AddSystemMessage(
    "You are a story development assistant. You help authors create " +
    "structured story outlines using the StoryCAD API. " +
    "When the user describes a story idea: " +
    "1) Create a new outline with CreateEmptyOutline, " +
    "2) Add characters, problems, scenes, and settings as needed, " +
    "3) Use beat sheets to structure the plot, " +
    "4) Link elements together, " +
    "5) Save the outline when asked.");

// Conversation loop
while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) break;

    history.AddUserMessage(input);
    var response = await chat.GetChatMessageContentAsync(
        history, settings, kernel);
    history.AddAssistantMessage(response.Content!);
    Console.WriteLine($"\nAssistant: {response.Content}");
}
```

```python
# Initialize StoryCADLib (BootStrapper.Initialise + Ioc.Default.GetRequiredService
# collapse into one call).
from storycad import StoryCAD

sc = StoryCAD(headless=True)

# (no direct Python equivalent — building a Kernel, registering the API as an SK
# plugin, IChatCompletionService, OpenAIPromptExecutionSettings, ChatHistory, and
# the automatic-function-calling chat loop are all .NET / Semantic Kernel
# concepts. The `sc` object exposes the same story-building methods the agent
# would call autonomously, e.g.:)
overview = sc.create_empty_outline("My Story", "Jake", template_index="2")[0]
hero = sc.add_element(sc.item_type.Character, overview, "Protagonist")
problem = sc.add_element(sc.item_type.Problem, overview, "Central Problem")
sc.apply_beat_sheet_to_problem(problem, "Save The Cat")
sc.write_outline("/tmp/agent_demo.stbx")
```

</div>

In this example, the LLM autonomously calls `CreateEmptyOutline`, `AddElement`, `UpdateElementProperty`, `ApplyBeatSheetToProblem`, and other API methods as it builds the outline through conversation.

## Multi-Step Agent Patterns

### Analysis Then Modification

An agent that reads an existing outline, analyzes it, and suggests or applies improvements:

<div class="code-tabs" markdown="1">

```csharp
// Open an existing outline
await api.OpenOutline("my-story.stbx");

// The LLM can now:
// 1. Call GetAllElements to see the full outline
// 2. Call SearchForReferences to find orphan elements
// 3. Call GetKeyQuestions to identify undeveloped areas
// 4. Call UpdateElementProperty to fill in gaps
// 5. Call ApplyBeatSheetToProblem to add structure
```

```python
# Open an existing outline
sc.open_outline("my-story.stbx")

# The same methods are available on `sc`:
# 1. sc.get_all_elements() to see the full outline
# 2. sc.search_for_references(element) to find orphan elements
# 3. sc.get_key_questions(element_type) to identify undeveloped areas
# 4. sc.update_element_property(element, "Prop", "value") to fill in gaps
# 5. sc.apply_beat_sheet_to_problem(problem, "Save The Cat") to add structure
```

</div>

### Resource-Driven Development

An agent that uses StoryCAD's built-in writing tools to guide story development:

<div class="code-tabs" markdown="1">

```csharp
// The LLM can combine resource APIs with element creation:
// 1. GetMasterPlotNames → choose a plot pattern
// 2. GetMasterPlotScenes → get the scene breakdown
// 3. AddElement (Scene) for each plot beat
// 4. GetConflictCategories → build character conflicts
// 5. ApplyConflictToProtagonist → apply to problems
// 6. GetKeyQuestions → guide deeper element development
```

```python
# The same resource methods can be combined with element creation on `sc`:
# 1. sc.get_master_plot_names() → choose a plot pattern
# 2. sc.get_master_plot_scenes(plot) → get the scene breakdown
# 3. sc.add_element(sc.item_type.Scene, overview, name) for each plot beat
# 4. sc.get_conflict_categories() → build character conflicts
# 5. sc.apply_conflict_to_protagonist(problem, "text") → apply to problems
# 6. sc.get_key_questions(element_type) → guide deeper element development
```

</div>

## Tips for System Prompts

When building SK agents with the StoryCAD plugin:

- **Guide the workflow order**: Instruct the LLM to create the outline first, then add elements, then link them. Without guidance, it may try to link elements before they exist.
- **Explain GUID references**: The LLM needs to understand that `Protagonist`, `Setting`, and similar fields take GUIDs of other elements, not text values.
- **Suggest resource usage**: Prompt the LLM to use `GetExamples`, `GetConflictCategories`, and other resource methods rather than inventing values.
- **Handle OperationResult**: Instruct the LLM to check `IsSuccess` before proceeding and to report `ErrorMessage` when operations fail.

## NuGet Dependencies

To use Semantic Kernel with StoryCADLib, add these packages:

```xml
<PackageReference Include="StoryCADLib" Version="4.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.*" />
```

For OpenAI specifically:

```xml
<PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.*" />
```

See the [Story Diagnostic Agent](../samples/story-diagnostic-agent.md) and [StoryCADCritter](../samples/storycad-critter.md) samples for complete working examples.
