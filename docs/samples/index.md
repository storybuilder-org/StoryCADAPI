# Samples

Five self-contained console applications that show you the StoryCADLib API in action. The first three use the core API only; the last two bring in Semantic Kernel for LLM-powered analysis.

| Sample | Type | What You'll Learn |
|--------|------|-------------------|
| [Story Graph Basics](story-graph-basics.md) | Core | Create, populate, link, save, and reload an outline |
| [Story Metrics](story-metrics.md) | Core | Query an outline for analytics and statistics |
| [Consistency Validation](consistency-validation.md) | Core | Detect structural issues in an outline |
| [Story Diagnostic Agent](story-diagnostic-agent.md) | Semantic Kernel | LLM-powered structural diagnosis |
| [Automated Critique](automated-critique.md) | Semantic Kernel | LLM-scored craft evaluation with rubric |

## Prerequisites

Every sample needs:

- **.NET 10.0 SDK**
- **StoryCADLib** -- each sample references it via `ProjectReference` to a local checkout of the [StoryCAD](https://github.com/storybuilder-org/StoryCAD) repository

The two Semantic Kernel samples also need:

- An **OpenAI API key** set as the `OPENAI_API_KEY` environment variable
- Internet access for OpenAI API calls

## Running a Sample

Same pattern for all of them:

```bash
cd <sample-folder>
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

For the Semantic Kernel samples, set your API key first:

```bash
export OPENAI_API_KEY="your-key-here"
```

## Repository Layout

Each sample lives in its own folder at the root of the [StoryCADAPI](https://github.com/storybuilder-org/StoryCADAPI) repository. You'll find four files inside:

```
SampleName/
  global.json          # SDK and Uno.Sdk versions
  SampleName.csproj    # Uno.Sdk console app targeting net10.0-desktop
  Program.cs           # Complete sample code
  README.md            # Quick reference
```

## API Coverage

Here's which API methods each sample exercises, so you can jump straight to the one that covers what you need:

| Method | Samples |
|--------|---------|
| `CreateEmptyOutline` | All |
| `AddElement` | All |
| `UpdateElementProperties` | All |
| `AddCastMember` | All except Diagnostic Agent |
| `AddRelationship` | Story Graph Basics |
| `GetAllElements` | All |
| `GetElementsByType` | Story Graph Basics, Story Metrics, Consistency Validation |
| `GetElement` | Story Graph Basics, Diagnostic Agent, Automated Critique |
| `GetStoryElement` | Story Metrics, Consistency Validation |
| `WriteOutline` | Story Graph Basics |
| `OpenOutline` | Story Graph Basics |
| `SearchForReferences` | Story Metrics, Consistency Validation |
| `GetKeyQuestionElements` | Automated Critique |
| `GetKeyQuestions` | Automated Critique |
