# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**StoryCADAPI** is the documentation and samples repository for [StoryCADLib](https://www.nuget.org/packages/StoryCADLib), the core library powering StoryCAD. It contains:

- **docs/** — Reference documentation (Jekyll "Just the Docs" GitHub Pages site)
- **samples/** — Runnable .NET 10 sample applications
- **StoryCADMcp/** — MCP server exposing StoryCADLib tools to AI agents
- **StoryCADCli/** — Console CLI wrapper around the API
- **StoryCADChat/** — Console app for natural-language interaction with outlines via LLM
- **HeadlessTest/** — 7-step round-trip headless API verification harness

Tracked as sub-issues of **StoryCAD#1246** (Release 4.2 API samples). Active issue: **#13** (Outliner sample hardening).

## Repository Setup

Samples reference StoryCADLib via `ProjectReference` — both repos must be siblings:

```
dev/src/
  StoryCAD/       # Main application + StoryCADLib
  StoryCADAPI/    # This repo
```

## Architecture

### Samples

| Sample | API key | Description |
|--------|---------|-------------|
| `StoryGraphBasics` | No | Create, populate, link, save, reload an outline |
| `StoryMetrics` | No | Element counts, character appearances, setting usage |
| `ConsistencyValidation` | No | 6 validation checks (orphan characters, unused settings, etc.) |
| `StoryDiagnosticAgent` | Yes | LLM-powered diagnosis of pacing, passive protagonist, plot holes |
| `StoryCADCritter` | Yes | LLM scores outline against 5 craft criteria |
| `Outliner` | Yes | Uno app — converts prose documents to `.stbx` outlines |

### Key patterns

**OperationResult**: all API methods return `OperationResult<T>` — always check `IsSuccess` before using `Payload`.

**Headless mode**: `BootStrapper.Initialise(headless: true)` configures StoryCADLib without a UI. Required in every sample entry point.

**NuGet vs ProjectReference**: controlled by `UseStoryCADLibNuGet` in `Directory.Build.props`. Currently `true` (NuGet, version pinned by `StoryCADLibVersion`). Set to `false` to use the sibling-clone source.

### Outliner sample architecture

`samples/Outliner/` is a Uno app:

- `ProseDocumentReader` — reads `.docx` and `.txt` prose files
- `ProseAnalyzer` — calls OpenAI via Semantic Kernel, returns `OnePassResponse`
- `OutlineBuilder` — maps `OnePassResponse` fields to StoryCADLib API calls
- `OutlineRunner` — shared pipeline (single-mode, batch-mode, and tests all use this)
- `OutlinerPreferences` / `PreferencesService` — persisted to `%LocalAppData%/Outliner/preferences.json`
- Pages: `ContentPage`, `BatchPage`, `SettingsPage` with matching ViewModels

Prompt is at `samples/Outliner/Outliner/Prompts/OnePassSystemPrompt.md` (current: v4).

Test strategy: live-LLM end-to-end tests are in `ManualTests/Outliner_End_To_End_Test.md`.

### MCP server

`StoryCADMcp/` exposes StoryCADLib operations as MCP tools for AI agent integration. Tests in `StoryCADMcp.Tests/`.

## Documentation (Jekyll / Just the Docs)

The docs site lives in `docs/` and is a Jekyll [Just the Docs](https://just-the-docs.com/) site — it is no longer docfx, and there is no `docfx.json`. The API reference pages under `docs/api/` are hand-authored Markdown. The site builds and deploys to GitHub Pages via `.github/workflows/deploy-docs.yml` on every push to `main` that touches `docs/`.

Serving locally requires Ruby + Bundler:

```powershell
# Preferred: wrapper script. Installs gems on first run, then serves with livereload.
# Serves at http://localhost:4000/StoryCADAPI/ (the site uses a /StoryCADAPI baseurl).
pwsh serve-docs.ps1 [port]

# Equivalent manual steps, run from the docs/ directory:
cd docs
bundle install                 # first run only
bundle exec jekyll serve --livereload
```

GitHub Pages deployment is disabled until the 4.0 store release.

## Build and test

The repo has no solution file for the whole repo. Each sample builds by itself.

```powershell
# One sample
cd samples/StoryGraphBasics
dotnet build -f net10.0-desktop

# The Outliner and StoryCADCritter samples
scripts\build-outliner.cmd
scripts\build-critter.cmd
```

There are three MSTest projects: `StoryCADMcp.Tests`, `samples/Outliner/OutlinerTests` and `samples/StoryCADCritter/StoryCADCritterTests`. The repo states no test command.

The development process is not in this file.
