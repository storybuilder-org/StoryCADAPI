---
layout: default
title: "Quick Start"
parent: "Getting Started"
nav_order: 2
---

# Quick Start

Build your first story outline in 5 minutes — the classic Hello World, applied to StoryCADLib.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- StoryCADLib package installed (see [Installation](index.md))

## Step 1: Set Up Your Project

<div class="code-tabs" data-tabs="C#|Python" markdown="1">

```bash
# Create a console project and add the package
dotnet new console -n HelloStoryCAD
cd HelloStoryCAD
dotnet add package StoryCADLib
```

```bash
# Create a folder for your script, then make sure the
# `storycad` module is importable in your environment
mkdir hello-storycad && cd hello-storycad
```

</div>

## Step 2: Write the Code

Add the following code — in `Program.cs` for C#, or a file such as `hello_world.py` for Python:

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// 1. Initialize the service container (headless = no UI)
BootStrapper.Initialise(headless: true);

// 2. Get the API instance
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// 3. Create an empty outline
var outlineResult = await api.CreateEmptyOutline("Hello World", "Your Name", "0");
if (!outlineResult.IsSuccess)
{
    Console.WriteLine($"Error: {outlineResult.ErrorMessage}");
    return;
}
Console.WriteLine($"Outline created with {outlineResult.Payload.Count} elements.");

// 4. Add a protagonist
var characterResult = api.CreateNewElement("Character");
if (characterResult.IsSuccess)
{
    var characterGuid = characterResult.Payload;
    api.UpdateElementProperty(characterGuid, "Name", "Alex");
    api.UpdateElementProperty(characterGuid, "Role", "Protagonist");
    Console.WriteLine("Character added.");
}

// 5. Add an opening scene
var sceneResult = api.CreateNewElement("Scene");
if (sceneResult.IsSuccess)
{
    var sceneGuid = sceneResult.Payload;
    api.UpdateElementProperty(sceneGuid, "Name", "The Call to Adventure");
    api.UpdateElementProperty(sceneGuid, "Setting", "A quiet village");
    Console.WriteLine("Scene added.");
}

// 6. Save the outline
var saveResult = await api.WriteOutline("hello-world.stbx");
if (saveResult.IsSuccess)
{
    Console.WriteLine("Saved to hello-world.stbx");
}
```

```python
from storycad import StoryCAD

# 1. Initialize (headless = no UI). This object is the API entry point.
sc = StoryCAD(headless=True)

# 2. Create an empty outline; [0] is the story overview element.
overview = sc.create_empty_outline("Hello World", "Your Name", template_index="0")[0]
print(f"Outline created with {len(sc.get_all_elements())} elements.")

# 3. Add a protagonist
alex = sc.add_element(sc.item_type.Character, overview, "Alex")
sc.update_element_property(alex, "Role", "Protagonist")
print("Character added.")

# 4. Add an opening scene
sc.add_element(sc.item_type.Scene, overview, "The Call to Adventure")
print("Scene added.")

# 5. Save the outline
sc.write_outline("hello-world.stbx")
print("Saved to hello-world.stbx")
```

</div>

## Step 3: Run It

<div class="code-tabs" data-tabs="C#|Python" markdown="1">

```bash
dotnet run
```

```bash
python hello_world.py
```

</div>

Expected output:

```
Outline created with 1 elements.
Character added.
Scene added.
Saved to hello-world.stbx
```

## Step 4: Open in StoryCAD

Open `hello-world.stbx` in the StoryCAD application to see your outline with the character and scene you just created.

## Understanding the Code

### Initialization

<div class="code-tabs" markdown="1">

```csharp
BootStrapper.Initialise(headless: true);
```

```python
sc = StoryCAD(headless=True)
```

</div>

This prepares all StoryCADLib services. In C# it sets up the dependency injection container; in Python, constructing `StoryCAD(headless=True)` does the equivalent. The `headless` flag configures the library for non-UI use (console apps, web APIs, batch tools).

### Getting the API

<div class="code-tabs" markdown="1">

```csharp
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

```python
# The StoryCAD object returned by the constructor *is* the API entry point.
sc = StoryCAD(headless=True)
```

</div>

`StoryCADApi` is the main entry point for all API operations. In C# it's resolved from the DI container; in Python the `StoryCAD(...)` object you constructed *is* the API, so there's no separate lookup step.

### Creating an Outline

<div class="code-tabs" markdown="1">

```csharp
var result = await api.CreateEmptyOutline("Hello World", "Your Name", "0");
```

```python
result = sc.create_empty_outline("Hello World", "Your Name", template_index="0")
```

</div>

Parameters:
- `title` — the story title.
- `author` — the author name.
- `templateIndex` — `"0"` for blank, `"1"` for a basic template.

### The OperationResult Pattern

In C#, every API method returns `OperationResult<T>`; always check `IsSuccess` before reading `Payload`. The Python wrapper takes the opposite approach — methods return their value directly and raise an exception on failure, so you use `try`/`except` instead:

<div class="code-tabs" markdown="1">

```csharp
if (result.IsSuccess)
{
    // Access result.Payload for the returned data
    var guids = result.Payload; // List<Guid> of created elements
}
else
{
    // Access result.ErrorMessage for error details
    Console.WriteLine(result.ErrorMessage);
}
```

```python
try:
    # The return value is the data directly (e.g. the list of created elements)
    elements = sc.create_empty_outline("Hello World", "Your Name", template_index="0")
except Exception as error:
    # The wrapper raises instead of returning an error result
    print(error)
```

</div>

### Saving

<div class="code-tabs" markdown="1">

```csharp
await api.WriteOutline("hello-world.stbx");
```

```python
sc.write_outline("hello-world.stbx")
```

</div>

Saves the current outline to the specified path. The `.stbx` extension is the StoryCAD file format (JSON-based).

## Next Steps

- [API Reference](../api/index.md) — full method documentation.
- [Samples](../samples/index.md) — five working sample applications, starting with [Story Graph Basics](../samples/story-graph-basics.md) as the foundational walkthrough.
