# Quick Start

Build your first story outline in 5 minutes.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- StoryCADLib package installed (see [Installation](index.md))

## Step 1: Create a Console Project

```bash
dotnet new console -n MyStoryApp
cd MyStoryApp
dotnet add package StoryCADLib
```

## Step 2: Write the Code

Replace `Program.cs` with:

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize StoryCADLib
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Create a new outline
var outlineResult = await api.CreateEmptyOutline("The Hero's Journey", "Your Name", "0");
if (!outlineResult.IsSuccess)
{
    Console.WriteLine($"Error: {outlineResult.ErrorMessage}");
    return;
}
Console.WriteLine("Outline created!");

// Add a protagonist
var characterResult = api.CreateNewElement("Character");
if (characterResult.IsSuccess)
{
    var characterGuid = characterResult.Payload;
    api.UpdateElementProperty(characterGuid, "Name", "Alex");
    api.UpdateElementProperty(characterGuid, "Role", "Protagonist");
    Console.WriteLine("Character added!");
}

// Add an opening scene
var sceneResult = api.CreateNewElement("Scene");
if (sceneResult.IsSuccess)
{
    var sceneGuid = sceneResult.Payload;
    api.UpdateElementProperty(sceneGuid, "Name", "The Call to Adventure");
    api.UpdateElementProperty(sceneGuid, "Setting", "A quiet village");
    Console.WriteLine("Scene added!");
}

// Save the outline
var saveResult = await api.WriteOutline("hero-journey.stbx");
if (saveResult.IsSuccess)
{
    Console.WriteLine("Outline saved to hero-journey.stbx");
}
```

## Step 3: Run It

```bash
dotnet run
```

Expected output:
```
Outline created!
Character added!
Scene added!
Outline saved to hero-journey.stbx
```

## Step 4: Open in StoryCAD

Open `hero-journey.stbx` in the StoryCAD application to see your outline with the character and scene you created.

## Next Steps

- [Hello World Sample](hello-world.md) - Detailed walkthrough of the minimal example
- [API Reference](../api/index.md) - Explore all available methods
- [Samples](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples) - More complete examples
