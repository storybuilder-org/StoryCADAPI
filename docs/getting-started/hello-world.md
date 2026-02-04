# Hello World

A minimal example demonstrating the core StoryCADLib API.

## The Code

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// 1. Initialize the service container
BootStrapper.Initialise(headless: true);

// 2. Get the API instance
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();

// 3. Create an empty outline
var result = await api.CreateEmptyOutline("Hello World", "Developer", "0");

// 4. Check the result
if (result.IsSuccess)
{
    Console.WriteLine($"Success! Created {result.Payload.Count} elements.");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}

// 5. Save the outline
await api.WriteOutline("hello-world.stbx");
```

## Understanding the Code

### Initialization

```csharp
BootStrapper.Initialise(headless: true);
```

This sets up the dependency injection container with all StoryCADLib services. The `headless: true` parameter configures it for non-UI use.

### Getting the API

```csharp
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();
```

The `SemanticKernelApi` class is the main entry point for all API operations. It's registered in the DI container by `BootStrapper.Initialise()`.

### Creating an Outline

```csharp
var result = await api.CreateEmptyOutline("Hello World", "Developer", "0");
```

Parameters:
- `title` - The story title
- `author` - The author name
- `templateIndex` - "0" for blank, "1" for basic template

### The OperationResult Pattern

All API methods return `OperationResult<T>`:

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

## Running the Sample

The complete sample is available at:
[samples/getting-started/HelloStoryCAD](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples/getting-started/HelloStoryCAD)

```bash
cd samples/getting-started/HelloStoryCAD
dotnet run
```

## Next Steps

- [Quick Start](quick-start.md) - More complete example with characters and scenes
- [API Reference](../api/index.md) - Full method documentation
