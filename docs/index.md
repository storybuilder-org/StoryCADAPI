---
layout: default
title: Home
nav_order: 1
description: "Build story outlines programmatically with the StoryCADLib API."
permalink: /
---

# StoryCAD API
{: .no_toc }

Programmatic story outlining for .NET. StoryCADLib lets you build, analyze, and transform `.stbx` story outlines from your own code, in console apps, web APIs, batch jobs, and AI agents, with no UI required.

It's the same engine that powers [StoryCAD](https://github.com/storybuilder-org/StoryCAD), exposed as a headless library and published on [NuGet](https://www.nuget.org/packages/StoryCADLib).

## What StoryCAD does

- **Manage outlines**: create, open, and save `.stbx` story outlines.
- **Work with elements**: add and edit characters, scenes, problems, settings, and more, then search and link them together.
- **Apply story structure**: drive beat sheets, master plots, and dramatic situations from built-in writing-craft data.
- **Run headless**: use it in console apps, web APIs, and batch pipelines without a window.
- **Integrate with AI**: pre-built Semantic Kernel functions and an MCP server expose the API to Claude Desktop, Claude Code, and other agents.

Every operation returns an `OperationResult<T>`. Check `IsSuccess` before reading `Payload`.

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

## Get started

- [Installation](getting-started/installation.md): add the library to your project.
- [Quick Start](getting-started/quick-start.md): build your first outline (Hello World) in 5 minutes.

## Where to find more

- [Concepts](concepts/story-model.md): the story model, element types, and resource data.
- [Operations](operations/search.md): search, resource workflows, and beat sheet operations.
- [Samples](samples/index.md): working code examples.
- [Advanced](advanced/semantic-kernel.md): Semantic Kernel integration and the MCP server.
- [API Reference](api/index.md): complete technical documentation.

## About StoryCAD

[StoryCAD](https://github.com/storybuilder-org/StoryCAD) is a free, open-source outlining tool for fiction writers. StoryCADLib is the core library that powers it, available as a NuGet package for developers who want to build tools that work with story outlines.

## License

StoryCADLib is released under the [GNU GPL v3](https://www.gnu.org/licenses/gpl-3.0.en.html) license.
