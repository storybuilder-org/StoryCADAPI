# StoryDiagnosticAgent

Uses Semantic Kernel + an LLM to analyze a story outline and diagnose structural issues.

## What It Does

1. Reads `OPENAI_API_KEY` from environment variables (fails fast if missing)
2. Initializes StoryCADLib in headless mode and builds a Semantic Kernel with OpenAI chat completion
3. Creates a story outline with intentional structural issues:
   - Passive protagonist (no goals or motives)
   - Pacing problems (action front-loaded, filler scenes at the end)
   - Missing reversals/turning points
   - Unresolved plot threads
4. Serializes all story elements via `GetElement` for LLM context
5. Sends a system prompt (StoryCAD concepts) and user prompt (serialized outline) to the LLM
6. Prints the AI-generated diagnostic report

## How to Run

```bash
# Set your API key
export OPENAI_API_KEY="your-key-here"

# Optionally set model (defaults to gpt-4o-mini)
export OPENAI_MODEL="gpt-4o-mini"

dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with properties |
| `UpdateElementProperties` | Set protagonist, antagonist, outcomes |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | List all elements for serialization |
| `GetElement` | Get full serialized data for each element |

### Semantic Kernel Usage

| Component | Purpose |
|-----------|---------|
| `Kernel.CreateBuilder().AddOpenAIChatCompletion()` | Configure SK with OpenAI |
| `IChatCompletionService` | Single-shot chat completion |
| `ChatHistory` | System + user message for structured prompting |

## Prerequisites

- .NET 10.0 SDK
- StoryCADLib project reference
- `OPENAI_API_KEY` environment variable set
- Internet access for OpenAI API calls
