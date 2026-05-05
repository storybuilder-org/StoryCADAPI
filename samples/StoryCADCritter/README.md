# StoryCADCritter

Uses Semantic Kernel + an LLM to evaluate a story outline against craft principles with a scored checklist.

## What It Does

1. Reads `OPENAI_API_KEY` from environment variables (fails fast if missing)
2. Initializes StoryCADLib and Semantic Kernel with OpenAI chat completion
3. Creates a fleshed-out outline ("The Last Lighthouse Keeper") with:
   - 3 Characters (with roles, flaws, backstory)
   - 2 Problems (with protagonist/antagonist goals, motives, theme, premise)
   - 3 Settings (with locale, season)
   - 5 Scenes (with conflict, cast, outcomes)
4. Serializes all elements and gathers key questions via `GetKeyQuestionElements`/`GetKeyQuestions`
5. Sends to LLM with a scoring rubric for 5 craft criteria:
   - **Premise** (1-5): Clarity and compelling nature
   - **Character Arcs** (1-5): Flaws, goals, growth potential
   - **Scene Structure** (1-5): Conflict, stakes, outcomes
   - **Conflict** (1-5): Central conflict definition and opposition
   - **Theme** (1-5): Coherent thematic connections
6. Prints the scored critique report with recommendations

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
| `AddElement` (with properties) | Add elements with initial property values |
| `UpdateElementProperties` | Set goals, motives, theme, premise, etc. |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | List all elements for serialization |
| `GetElement` | Get full serialized data for each element |
| `GetKeyQuestionElements` | Discover element types with key questions |
| `GetKeyQuestions` | Get craft questions for each element type |

### Semantic Kernel Usage

| Component | Purpose |
|-----------|---------|
| `Kernel.CreateBuilder().AddOpenAIChatCompletion()` | Configure SK with OpenAI |
| `IChatCompletionService` | Single-shot chat completion |
| `ChatHistory` | System + user message with scoring rubric |

## Prerequisites

- .NET 10.0 SDK
- StoryCADLib project reference
- `OPENAI_API_KEY` environment variable set
- Internet access for OpenAI API calls
