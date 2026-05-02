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

## Repository Setup

Samples reference StoryCADLib via ProjectReference, so you need both repos as siblings:

```
dev/src/
  StoryCAD/          # Main application + StoryCADLib
  StoryCADAPI/       # This repo (docs + samples)
```

```bash
cd your-dev-directory
git clone https://github.com/storybuilder-org/StoryCAD.git
git clone https://github.com/storybuilder-org/StoryCADAPI.git
```

### Prerequisites

- .NET 10.0 SDK
- OpenAI API key (for Semantic Kernel samples only)

## Quick Start

### Installation (NuGet)

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

### Core Samples (no API key required)

| Sample | Description |
|--------|-------------|
| [StoryGraphBasics](samples/StoryGraphBasics/) | Create, populate, link, save and reload an outline (10 API methods) |
| [StoryMetrics](samples/StoryMetrics/) | Analytics dashboard: element counts, character appearances, setting usage |
| [ConsistencyValidation](samples/ConsistencyValidation/) | 6 validation checks detecting story issues (orphan characters, unused settings, etc.) |

### Semantic Kernel Samples (require `OPENAI_API_KEY`)

| Sample | Description |
|--------|-------------|
| [StoryDiagnosticAgent](samples/StoryDiagnosticAgent/) | LLM-powered diagnosis of pacing, passive protagonist, plot holes |
| [StoryCADCritter](samples/StoryCADCritter/) | LLM scores an outline against 5 craft criteria with rubric |

### Other

| Sample | Description |
|--------|-------------|
| [StoryCADChat](StoryCADChat/) | Console app for natural language interaction with outlines via LLM |
| [HeadlessTest](HeadlessTest/) | Verification harness: 7-step round-trip test of headless API |


### Building and Running Samples

All samples target `net10.0-desktop` and use Uno SDK:

```bash
cd samples/StoryGraphBasics
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

For Semantic Kernel samples, set your API key first:

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

## Building the Documentation Locally

The API reference is auto-generated from StoryCADLib's XML documentation using [docfx](https://dotnet.github.io/docfx/).

### Prerequisites

- [docfx](https://dotnet.github.io/docfx/) CLI tool (`dotnet tool install -g docfx`)
- docfx requires the .NET 9 runtime (its own dependency, separate from the project's .NET 10 target)
- On WSL, run docfx commands via `cmd.exe` since WSL may not have .NET 9 installed

### Workflow

**Step 1: Build StoryCADLib** (generates DLL + XML documentation)

```bash
# From the StoryCAD repo
dotnet build StoryCADLib/StoryCADLib.csproj -c Debug -f net10.0-windows10.0.22621
```

This produces:
- `StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.dll`
- `StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.xml`

**Step 2: Copy artifacts to `_assemblies/`**

```bash
# From the StoryCADAPI repo root
mkdir -p _assemblies
cp ../StoryCAD/StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.dll _assemblies/
cp ../StoryCAD/StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.xml _assemblies/
```

docfx reads the DLL for type metadata and the XML for documentation comments. This avoids
building the UNO SDK project directly (which docfx cannot do due to multi-targeting).

**Step 3: Generate and preview the site**

```bash
# Build only
cd docs
docfx docfx.json

# Build and serve locally (opens at http://localhost:8080)
docfx docfx.json --serve

# Stop the server (WSL — Linux tools can't see Windows processes)
taskkill.exe /F /IM docfx.exe
```

> **Note:** `InvalidAssemblyReference` warnings are expected. docfx cannot resolve
> StoryCADLib's dependencies (UNO, WinUI, Semantic Kernel, etc.) but generates
> correct API documentation regardless.

### CI Workflow

The GitHub Actions workflow (`.github/workflows/deploy-docs.yml`) automates this:
1. Checks out both StoryCADAPI and StoryCAD repos
2. Builds StoryCADLib via `dotnet build`
3. Copies DLL + XML to `_assemblies/`
4. Runs `docfx docfx.json`
5. Uploads the generated site as a build artifact

Deployment to GitHub Pages is disabled until the 4.0 store release.

## Related Projects

- [StoryCAD](https://github.com/storybuilder-org/StoryCAD) - The main application
- [Collaborator](https://github.com/storybuilder-org/Collaborator) - AI plugin for StoryCAD
- [StoryCAD Manual](https://storybuilder-org.github.io/StoryCAD/) - User documentation

## License

GNU GPL v3 - See [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please read the contribution guidelines in the main [StoryCAD repository](https://github.com/storybuilder-org/StoryCAD).
