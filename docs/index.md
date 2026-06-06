---
layout: default
title: Home
nav_order: 1
description: "Build story outlines programmatically with the StoryCADLib API."
permalink: /
---

# StoryCAD API
{: .no_toc }

Build story outlines programmatically with the StoryCADLib API.

## Quick Example

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize in headless mode (no UI required)
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Create a new outline
var result = await api.CreateEmptyOutline("My Story", "Author Name", "0");
if (result.IsSuccess)
{
    Console.WriteLine($"Created outline with {result.Payload.Count} elements");
}

// Save to file
await api.WriteOutline("my-story.stbx");
```

```python
from storycad import StoryCAD

# Initialize in headless mode (no UI required)
sc = StoryCAD(headless=True)

# Create a new outline (raises on error instead of returning a result)
elements = sc.create_empty_outline("My Story", "Author Name", template_index="0")
print(f"Created outline with {len(elements)} elements")

# Save to file
sc.write_outline("my-story.stbx")
```

</div>

## Features

- **Outline Management** - Create, open, save story outlines (.stbx files)
- **Element CRUD** - Add characters, scenes, problems, settings, and more
- **Structure Tools** - Beat sheets, master plots, dramatic situations
- **Headless Mode** - Use in console apps, web APIs, batch processing
- **Semantic Kernel Ready** - Pre-built functions for AI agent integration
- **MCP Server** - Expose the API to Claude Desktop, Claude Code, and other MCP clients

## Getting Started

- [Installation](getting-started/installation.md) - Add the library to your project
- [Quick Start](getting-started/quick-start.md) - Build your first outline (Hello World) in 5 minutes

## Documentation

- [Concepts](concepts/story-model.md) - Understand the story model, element types, and resource data
- [Operations](operations/search.md) - Search, resource workflows, and beat sheet operations
- [Samples](samples/index.md) - Working code examples
- [Advanced](advanced/semantic-kernel.md) - Semantic Kernel integration and MCP server
- [API Reference](api/index.md) - Complete technical documentation

## About StoryCAD

[StoryCAD](https://github.com/storybuilder-org/StoryCAD) is a free, open-source outlining tool for fiction writers. StoryCADLib is the core library that powers StoryCAD, available as a NuGet package for developers who want to build tools that work with story outlines.

## License

StoryCADLib is released under the [GNU GPL v3](https://www.gnu.org/licenses/gpl-3.0.en.html) license.
