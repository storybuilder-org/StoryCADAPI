---
layout: default
title: "Outliner"
parent: "Samples"
nav_order: 5
---

# Outliner

Reads a prose document and generates a StoryCAD outline from it using Semantic Kernel and an LLM. This is the inverse of the normal StoryCAD workflow: instead of building an outline first and writing prose from it, the Outliner reverse-engineers a structural outline from finished or draft text.

> [!NOTE]
> This sample requires an OpenAI API key. Set the `OPENAI_API_KEY` environment variable before running.

[View source on GitHub](https://github.com/storybuilder-org/StoryCADAPI/tree/main/samples/Outliner)

## What It Demonstrates

The sample accepts a `.txt`, `.docx`, or `.pdf` file and sends the full prose to an LLM with a structured system prompt (`Prompts/OnePassSystemPrompt.md`). The LLM returns a JSON response describing the story's characters, settings, problems, and scenes. The `OutlineBuilder` class then constructs a live StoryCAD outline from that response using the StoryCADLib API, and saves it as a `.stbx` file.

This is a Uno desktop application targeting both `net10.0-windows10.0.22621` and `net10.0-desktop`, so it runs on Windows (WinUI) and macOS (Skia).

## API Methods Used

| Method | Purpose |
|--------|---------|
| `CreateEmptyOutline` | Create a new story from template |
| `AddElement` | Add Character, Setting, Problem, Scene elements |
| `UpdateElementProperties` | Set title, author, premise, and other overview fields |
| `AddCastMember` | Link characters to scenes |

### Semantic Kernel Components

| Component | Purpose |
|-----------|---------|
| `Kernel.CreateBuilder().AddOpenAIChatCompletion()` | Configure SK with OpenAI |
| `IChatCompletionService` | Single-shot chat completion |
| `ChatHistory` | System prompt + full prose as user message |

## How to Run

```bash
export OPENAI_API_KEY="your-key-here"
```

Build and run on the desktop target:

```bash
cd samples/Outliner/Outliner
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

On Windows, build the WinUI target:

```bash
cd samples/Outliner/Outliner
dotnet build -f net10.0-windows10.0.22621 -p:Platform=x64
```

From the repo root, `.\scripts\build-outliner.cmd` runs this build.

When the app opens, select a prose file (`.txt`, `.docx`, or `.pdf`) and an output path for the `.stbx`. The app extracts the text, calls the LLM, and builds the outline. A `.raw.json` file is written alongside the output with the full LLM response for inspection.

You can optionally set the model (defaults to `gpt-4o-mini`):

```bash
export OPENAI_MODEL="gpt-4o"
```

## Input Requirements

- The full prose must fit within the model's context window. For most short stories and novelettes, `gpt-4o-mini` (128k context) is sufficient.
- `.docx` files are read via `DocumentFormat.OpenXml`; `.pdf` files via `PdfPig`. Complex formatting is stripped; plain text is what reaches the LLM.
