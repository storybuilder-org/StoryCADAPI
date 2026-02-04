---
_layout: landing
---

# StoryCAD API

Build story outlines programmatically with the StoryCADLib API.

## Quick Example

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize in headless mode (no UI required)
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();

// Create a new outline
var result = await api.CreateEmptyOutline("My Story", "Author Name", "0");
if (result.IsSuccess)
{
    Console.WriteLine($"Created outline with {result.Payload.Count} elements");
}

// Save to file
await api.WriteOutline("my-story.stbx");
```

## Features

- **Outline Management** - Create, open, save story outlines (.stbx files)
- **Element CRUD** - Add characters, scenes, problems, settings, and more
- **Structure Tools** - Beat sheets, master plots, dramatic situations
- **Headless Mode** - Use in console apps, web APIs, batch processing
- **Semantic Kernel Ready** - Pre-built functions for AI agent integration

## Getting Started

- [Installation](getting-started/index.md) - Install the NuGet package
- [Quick Start](getting-started/quick-start.md) - Build your first outline in 5 minutes
- [Hello World Sample](getting-started/hello-world.md) - Minimal working example

## Documentation

- [API Reference](api/index.md) - Complete technical documentation
- [Samples](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples) - Working code examples

## About StoryCAD

[StoryCAD](https://github.com/storybuilder-org/StoryCAD) is a free, open-source outlining tool for fiction writers. StoryCADLib is the core library that powers StoryCAD, available as a NuGet package for developers who want to build tools that work with story outlines.

## License

StoryCADLib is released under the [GNU GPL v3](https://www.gnu.org/licenses/gpl-3.0.en.html) license.
