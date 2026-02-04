# StoryCADAPI

Documentation and samples for the [StoryCADLib](https://www.nuget.org/packages/StoryCADLib) API.

[![NuGet](https://img.shields.io/nuget/v/StoryCADLib.svg)](https://www.nuget.org/packages/StoryCADLib)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

## What is StoryCADLib?

StoryCADLib is the core library that powers [StoryCAD](https://github.com/storybuilder-org/StoryCAD), a free outlining tool for fiction writers. The library is available as a NuGet package for developers who want to:

- Build tools that create or manipulate story outlines (.stbx files)
- Integrate story structure into AI agents via Semantic Kernel
- Automate story analysis and validation
- Create custom writing assistants

## Documentation

**[View Full Documentation](https://storybuilder-org.github.io/StoryCADAPI/)** *(coming soon)*

- [Getting Started](docs/getting-started/index.md) - Installation and first API call
- [Quick Start Tutorial](docs/getting-started/quick-start.md) - Build an outline in 5 minutes
- [API Reference](docs/api/index.md) - Complete method documentation

## Quick Start

### Installation

```bash
dotnet add package StoryCADLib
```

### Minimal Example

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize (once at startup)
ServiceLocator.Initialize(headless: true);

// Get the API
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();

// Create a new outline
var result = await api.CreateEmptyOutline("My Story", "Author Name", "0");
if (result.IsSuccess)
{
    Console.WriteLine($"Created outline with {result.Payload.Count} elements");
}

// Add a character
var charResult = api.AddElement(
    StoryItemType.Character,
    result.Payload[0].ToString(),  // Parent GUID (story overview)
    "Alex",
    "The protagonist");

// Save to file
await api.WriteOutline("my-story.stbx");
```

## Samples

| Sample | Description |
|--------|-------------|
| [StoryCADChat](StoryCADChat/) | Console app for natural language interaction with outlines via LLM |

### Running Samples

Samples that use LLMs require an OpenAI API key:

```bash
# Windows (PowerShell)
$env:OPENAI_API_KEY="your_key_here"

# Linux/Mac
export OPENAI_API_KEY=your_key_here
```

See [.env.example](.env.example) for all environment variables.

## Key Concepts

### OperationResult Pattern

All API methods return `OperationResult<T>` - always check `IsSuccess` before using `Payload`:

```csharp
var result = api.GetStoryElement(guid);
if (result.IsSuccess)
{
    var element = result.Payload;
    // Use element...
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### Headless Mode

`ServiceLocator.Initialize(headless: true)` configures the library for use without a UI:
- Console applications
- Web APIs
- Background services
- AI agent integrations

### Element Types

| Type | Description |
|------|-------------|
| `StoryOverview` | Root element with story metadata |
| `Problem` | Conflicts, themes, story questions |
| `Character` | Characters in the story |
| `Scene` | Individual scenes |
| `Setting` | Locations and places |
| `Folder` | Organizational containers |

## Related Projects

- [StoryCAD](https://github.com/storybuilder-org/StoryCAD) - The main application
- [Collaborator](https://github.com/storybuilder-org/Collaborator) - AI plugin for StoryCAD
- [StoryCAD Manual](https://storybuilder-org.github.io/StoryCAD/) - User documentation

## License

GNU GPL v3 - See [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please read the contribution guidelines in the main [StoryCAD repository](https://github.com/storybuilder-org/StoryCAD).
