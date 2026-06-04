---
layout: default
title: "StoryCADCritter"
parent: "Samples"
nav_order: 5
---

# StoryCADCritter

Uses Semantic Kernel and an LLM to evaluate a story outline against five craft criteria, producing a scored checklist with recommendations. This is the most advanced sample, combining the full breadth of StoryCADLib's query APIs with structured LLM prompting.

> [!NOTE]
> This sample requires an OpenAI API key. Set the `OPENAI_API_KEY` environment variable before running.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples/StoryCADCritter)

## What It Demonstrates

The sample creates a fleshed-out outline called "The Last Lighthouse Keeper" with 3 Characters (with roles, flaws, and backstory), 2 Problems (with goals, motives, theme, and premise), 3 Settings, and 5 Scenes (with conflict, cast, and outcomes). Unlike the Diagnostic Agent sample, this outline is intentionally well-constructed so the critique can demonstrate both praise and suggestions.

After building the outline, it serializes all elements and gathers key questions via `GetKeyQuestionElements` and `GetKeyQuestions` — these are the craft questions that StoryCAD uses to guide writers. The serialized outline and key questions are sent to the LLM along with a scoring rubric.

The LLM evaluates the outline on five criteria, each scored 1-5:

| Criterion | What It Evaluates |
|-----------|-------------------|
| **Premise** | Clarity and compelling nature of the story premise |
| **Character Arcs** | Flaws, goals, and growth potential |
| **Scene Structure** | Conflict, stakes, and outcomes in each scene |
| **Conflict** | Central conflict definition and strength of opposition |
| **Theme** | Coherent thematic connections across the outline |

This sample introduces two API methods not used elsewhere: `GetKeyQuestionElements` discovers which element types have key questions defined, and `GetKeyQuestions` retrieves those questions. Together they provide the craft framework that makes the critique domain-aware rather than generic.

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add elements with initial property values |
| `UpdateElementProperties` | Set goals, motives, theme, premise |
| `AddCastMember` | Link characters to scenes |
| `GetAllElements` | List all elements for serialization |
| `GetElement` | Get full serialized data for each element |
| `GetKeyQuestionElements` | Discover element types with key questions |
| `GetKeyQuestions` | Get craft questions for each element type |

### Semantic Kernel Components

| Component | Purpose |
|-----------|---------|
| `Kernel.CreateBuilder().AddOpenAIChatCompletion()` | Configure SK with OpenAI |
| `IChatCompletionService` | Single-shot chat completion |
| `ChatHistory` | System + user message with scoring rubric |

## How to Run

```bash
export OPENAI_API_KEY="your-key-here"

cd samples/StoryCADCritter
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

You can optionally set the model (defaults to `gpt-4o-mini`):

```bash
export OPENAI_MODEL="gpt-4o-mini"
```
