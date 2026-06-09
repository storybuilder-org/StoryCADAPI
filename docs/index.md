---
layout: default
title: Home
nav_order: 1
description: "Build story outlines programmatically with the StoryCADLib API."
permalink: /
---

# StoryCAD API
{: .no_toc }

<img src="{{ '/images/pencil-cartoon.gif' | relative_url }}" alt="The StoryCAD mascot, a writer shouldering a giant pencil" width="180" align="right">

Programmatic story outlining for .NET. StoryCADLib lets you build, analyze, and transform `.stbx` story outlines from your own code, in console apps, web APIs, batch jobs, and AI agents, with no UI required.

Every outline is a [StoryModel](concepts/story-model.md): a tree of typed [story elements](concepts/glossary.md#story-element), such as characters, scenes, problems, and settings, that each API call reads from or writes to.

It's the same engine that powers [StoryCAD](https://github.com/storybuilder-org/StoryCAD), a free, open-source outlining tool for fiction writers, exposed as a headless library and published on [NuGet](https://www.nuget.org/packages/StoryCADLib).

## What the API does

- **Manage outlines**: create, open, and save [`.stbx`](concepts/story-model.md#file-format) story outlines.
- **Work with elements**: add and edit characters, scenes, problems, settings, and more, then search and link them together.
- **Apply story structure**: drive [beat sheets](concepts/glossary.md#beat-sheet) and [master plots](concepts/glossary.md#master-plot) from built-in writing-craft data.
- **Run [headless](concepts/glossary.md#headless-mode)**: use it in console apps, web APIs, and batch pipelines without a window.
- **Integrate with AI**: pre-built Semantic Kernel functions and an MCP server expose the API to Claude Desktop, Claude Code, and other agents.

Every operation returns an [`OperationResult<T>`](concepts/glossary.md#operationresult). Check `IsSuccess` before reading `Payload`.

## Get started

- [Installation](getting-started/installation.md): add the library to your project.
- [Quick Start](getting-started/quick-start.md): build your first outline (Hello World) in 5 minutes.

## Where to find more

[Concepts](concepts/index.md) covers the data model and terminology: the StoryModel, the eleven element types, and the built-in resource data. The [Glossary](concepts/glossary.md) defines the terms used across this site. [Operations](operations/index.md) holds task-oriented how-tos for search, resource workflows, and beat sheets, while [Samples](samples/index.md) has runnable code and the [API Reference](api/index.md) documents every method. Semantic Kernel and MCP integration live under [Advanced](advanced/index.md).

## License

StoryCADLib is released under the [GNU GPL v3](https://www.gnu.org/licenses/gpl-3.0.en.html) license.
