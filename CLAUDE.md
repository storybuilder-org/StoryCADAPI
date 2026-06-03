# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**StoryCADAPI** is the documentation and samples repository for [StoryCADLib](https://www.nuget.org/packages/StoryCADLib), the core library powering StoryCAD. It contains:

- **docs/** — Reference documentation (docfx-generated GitHub Pages site)
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

## Build and Run

All samples target `net10.0-desktop` (Uno SDK):

```powershell
# Build a sample
dotnet build samples/StoryGraphBasics/StoryGraphBasics.csproj -f net10.0-desktop

# Run a sample
dotnet run --project samples/StoryGraphBasics/StoryGraphBasics.csproj -f net10.0-desktop

# Outliner requires x64 platform
dotnet build samples/Outliner/Outliner/Outliner.csproj -f net10.0-desktop -p:Platform=x64
dotnet run --project samples/Outliner/Outliner/Outliner.csproj -f net10.0-desktop -p:Platform=x64

# Run MCP server tests
dotnet test StoryCADMcp.Tests/StoryCADMcp.Tests.csproj -f net10.0-desktop

# Run Outliner tests
dotnet test samples/Outliner/OutlinerTests/OutlinerTests.csproj -f net10.0-desktop -p:Platform=x64
```

For Semantic Kernel samples, set the API key first:

```powershell
$env:OPENAI_API_KEY = "your_key_here"
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
| `Outliner` | Yes | Full Uno WinUI app — converts prose documents to `.stbx` outlines |

### Key patterns

**OperationResult**: all API methods return `OperationResult<T>` — always check `IsSuccess` before using `Payload`.

**Headless mode**: `ServiceLocator.Initialize(headless: true)` configures StoryCADLib without a UI. Required in every sample entry point.

**NuGet vs ProjectReference**: controlled by `UseStoryCADLibNuGet` in `Directory.Build.props`. Currently `true` (NuGet, version pinned by `StoryCADLibVersion`). Set to `false` to use the sibling-clone source.

### Outliner sample architecture

`samples/Outliner/` is a Uno WinUI app:

- `ProseDocumentReader` — reads `.docx` and `.txt` prose files
- `ProseAnalyzer` — calls OpenAI via Semantic Kernel, returns `OnePassResponse`
- `OutlineBuilder` — maps `OnePassResponse` fields to StoryCADLib API calls
- `OutlineRunner` — shared pipeline (single-mode, batch-mode, and tests all use this)
- `OutlinerPreferences` / `PreferencesService` — persisted to `%LocalAppData%/Outliner/preferences.json`
- Pages: `ContentPage`, `BatchPage`, `SettingsPage` with matching ViewModels

Prompt is at `samples/Outliner/Outliner/Prompts/OnePassSystemPrompt.md` (current: v4).

Test strategy: deterministic tests use `StubbedPipelineTests.cs` with `FakeChatCompletionService` and recorded fixture `Fixtures/Mirror, Mirror.raw.json`. Live-LLM tests are in `ManualTests/Outliner_End_To_End_Test.md`.

### MCP server

`StoryCADMcp/` exposes StoryCADLib operations as MCP tools for AI agent integration. Tests in `StoryCADMcp.Tests/`.

## Documentation (docfx)

```powershell
# Step 1: build StoryCADLib (from StoryCAD repo)
dotnet build StoryCADLib/StoryCADLib.csproj -c Debug -f net10.0-windows10.0.22621

# Step 2: copy artifacts
mkdir _assemblies
cp ../StoryCAD/StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.dll _assemblies/
cp ../StoryCAD/StoryCADLib/bin/Debug/net10.0-windows10.0.22621/StoryCADLib.xml _assemblies/

# Step 3: generate and serve (run via cmd.exe on WSL; docfx needs .NET 9)
cd docs && docfx docfx.json --serve
```

GitHub Pages deployment is disabled until the 4.0 store release.

## devdocs Index

| File | Purpose |
|------|---------|
| `devdocs/issue_13_status_log.md` | Session history for Outliner hardening (#13) |

## Status Log

`devdocs/issue_13_status_log.md` is the current active status log.

**Update it at the end of every working session**, or more frequently if directed.

## Standards

This repo follows the shared standards in `D:\dev\src\CLAUDE.md`:
- PIE workflow (Plan → Implement → Evaluate); plan must be approved before implementation
- Wiki at `D:\dev\src\StoryCADWiki\` is the authoritative cross-repo knowledge layer
- Agent usage required for complex tasks (>15 min OR >2 files OR architectural)
- Issue body checkboxes updated via `gh issue edit` as work progresses
- Use plain English and standard developer jargon; do not invent terms for tasks, code constructs, or concepts
