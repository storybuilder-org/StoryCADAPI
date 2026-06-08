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
BootStrapper.Initialise(headless: true);

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

`BootStrapper.Initialise(headless: true)` configures the library for use without a UI:
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

The documentation site lives in [`docs/`](docs/) and is a [Jekyll](https://jekyllrb.com/)
site using the [Just the Docs](https://just-the-docs.com/) theme. Pages are plain
Markdown with YAML front matter that drives the navigation (`title`, `parent`,
`nav_order`).

### Prerequisites

- Ruby 3.x and [Bundler](https://bundler.io/) (`gem install bundler`)

### Workflow

```bash
# From the repo root — installs gems into docs/ and serves with live reload
cd docs
bundle install
bundle exec jekyll serve --livereload
# → http://localhost:4000/StoryCADAPI/
```

On Windows you can use the helper script instead:

```powershell
pwsh serve-docs.ps1        # serves at http://localhost:4000
```

To produce the static site without serving:

```bash
cd docs
bundle exec jekyll build   # output in docs/_site/ (git-ignored)
```

### API Reference

The pages under `docs/api/` are hand-curated Markdown. The per-namespace model
reference (`docs/api/models*.md`) was converted from the StoryCADLib XML docs;
regenerate it if the public model surface changes.

### CI / Deployment

The GitHub Actions workflow (`.github/workflows/deploy-docs.yml`) builds the site
with Bundler/Jekyll and deploys it to GitHub Pages on every push to `main` that
touches `docs/`.

> **One-time setup:** in repo **Settings → Pages**, set **Source** to
> **GitHub Actions**. The site then publishes to
> <https://storybuilder-org.github.io/StoryCADAPI/>.

## Related Projects

- [StoryCAD](https://github.com/storybuilder-org/StoryCAD) - The main application
- [Collaborator](https://github.com/storybuilder-org/Collaborator) - AI plugin for StoryCAD
- [StoryCAD Manual](https://storybuilder-org.github.io/StoryCAD/) - User documentation

## License

GNU GPL v3 - See [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please read the contribution guidelines in the main [StoryCAD repository](https://github.com/storybuilder-org/StoryCAD).
