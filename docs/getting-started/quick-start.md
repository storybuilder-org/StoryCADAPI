# Quick Start

Build your first story outline in 5 minutes — the classic Hello World, applied to StoryCADLib.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- StoryCADLib package installed (see [Installation](index.md))

## Step 1: Create a Console Project

```bash
dotnet new console -n HelloStoryCAD
cd HelloStoryCAD
dotnet add package StoryCADLib
```

## Step 2: Write the Code

Replace `Program.cs` with:

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

## Step 3: Run It

```bash
dotnet run
```

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

```csharp
BootStrapper.Initialise(headless: true);
```

This sets up the dependency injection container with all StoryCADLib services. The `headless: true` parameter configures it for non-UI use (console apps, web APIs, batch tools).

### Getting the API

```csharp
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

`StoryCADApi` is the main entry point for all API operations. It's registered in the DI container by `BootStrapper.Initialise()`.

### Creating an Outline

```csharp
var result = await api.CreateEmptyOutline("Hello World", "Your Name", "0");
```

Parameters:
- `title` — the story title.
- `author` — the author name.
- `templateIndex` — `"0"` for blank, `"1"` for a basic template.

### The OperationResult Pattern

Every API method returns `OperationResult<T>`. Always check `IsSuccess` before reading `Payload`:

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

### Saving

```csharp
await api.WriteOutline("hello-world.stbx");
```

Saves the current outline to the specified path. The `.stbx` extension is the StoryCAD file format (JSON-based).

## Next Steps

- [API Reference](../api/index.md) — full method documentation.
- [Samples](../samples/index.md) — five working sample applications, starting with [Story Graph Basics](../samples/story-graph-basics.md) as the foundational walkthrough.
