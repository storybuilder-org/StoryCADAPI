# Story Diagnostic Agent

Uses Semantic Kernel and an LLM to analyze a story outline and produce a diagnostic report identifying structural issues. This is the first of two samples that demonstrate AI-powered story analysis.

> [!NOTE]
> This sample requires an OpenAI API key. Set the `OPENAI_API_KEY` environment variable before running.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/StoryDiagnosticAgent)

## What It Demonstrates

The sample creates an outline with intentional structural weaknesses — a passive protagonist with no goals or motives, front-loaded action with filler scenes at the end, missing reversals and turning points, and unresolved plot threads. It then serializes every element via `GetElement` and sends the full outline to an LLM along with a system prompt explaining StoryCAD concepts.

The LLM returns a diagnostic report identifying issues like pacing problems, passive characters, and missing story beats. This demonstrates how to combine StoryCADLib's data extraction APIs with Semantic Kernel's chat completion to build AI-powered analysis tools.

The key pattern is straightforward: extract structured data from the story graph, format it as context for the LLM, and let the model apply narrative craft knowledge that would be impractical to encode as rules.

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with properties |
| `UpdateElementProperties` | Set protagonist, antagonist, outcomes |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | List all elements for serialization |
| `GetElement` | Get full serialized data for each element |

### Semantic Kernel Components

| Component | Purpose |
|-----------|---------|
| `Kernel.CreateBuilder().AddOpenAIChatCompletion()` | Configure SK with OpenAI |
| `IChatCompletionService` | Single-shot chat completion |
| `ChatHistory` | System + user message for structured prompting |

## How to Run

```bash
export OPENAI_API_KEY="your-key-here"

cd StoryDiagnosticAgent
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

You can optionally set the model (defaults to `gpt-4o-mini`):

```bash
export OPENAI_MODEL="gpt-4o-mini"
```
