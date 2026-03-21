# StoryCADAPI Docs Tone Rewrite — Status

**Date**: 2026-02-20
**Branch**: `issue-1246-api-docs` (in StoryCADAPI repo at `/mnt/d/dev/src/StoryCADAPI`)
**Goal**: Rewrite all documentation pages for conversational Polly-style tone (pollydocs.org) while preserving all technical content.

## Completed

All 15 documentation markdown files have been rewritten with Polly-style tone:

### Landing + Getting Started (3 files)
- [x] `docs/index.md` — landing page
- [x] `docs/getting-started/index.md` — installation
- [x] `docs/getting-started/quick-start.md` — quick start tutorial

### Concepts (3 files)
- [x] `docs/concepts/story-model.md` — StoryModel structure, GUID refs, tree views
- [x] `docs/concepts/element-types.md` — all 11 element types with property tables
- [x] `docs/concepts/resource-data.md` — resource APIs, conflict builder, master plots, etc.

### Operations (3 files)
- [x] `docs/operations/search.md` — SearchForText, SearchForReferences, SearchInSubtree, RemoveReferences
- [x] `docs/operations/resource-workflows.md` — step-by-step code for conflict, key questions, master plots, stock scenes
- [x] `docs/operations/beat-sheets.md` — 12 beat sheet methods with complete example

### Advanced (2 files)
- [x] `docs/advanced/semantic-kernel.md` — SK plugin registration, agent building, system prompt tips
- [x] `docs/advanced/migration-3x-to-4x.md` — namespace/class/TFM changes, new methods, checklist

### API Reference + Samples (4 files)
- [x] `docs/api/index.md` — method quick-reference tables
- [x] `docs/api/operation-result.md` — OperationResult<T> class, factory methods, best practices
- [x] `docs/api/semantic-kernel-api.md` — full SemanticKernelApi class reference with signatures
- [x] `docs/samples/index.md` — 5 samples with API coverage matrix

## Remaining Work

### 1. Rebuild docfx site and review
```bash
cmd.exe /c "C:\Users\tcox\.dotnet\tools\docfx.exe D:\dev\src\StoryCADAPI\docs\docfx.json"
```
Then preview at `http://localhost:8088` (Python HTTP server from `D:\dev\src\StoryCADAPI\docs\_site`).

### 2. Visual review in browser
- Check all 15 pages render correctly
- Verify code blocks, tables, and links work
- Check navigation (TOC, breadcrumbs)
- Confirm landing page layout still looks right (custom CSS in `docs/public/main.css`)

### 3. hello-world.md
- `docs/getting-started/hello-world.md` — referenced in navigation but not yet reviewed/rewritten (may not exist yet or may need creation)

### 4. Commit changes
- All changes are uncommitted on `issue-1246-api-docs` branch
- No changes pushed to remote yet

### 5. Deploy to GitHub Pages
- Push branch and merge (or deploy from branch)

## Lessons Learned

- **`technical-writer` agents cannot read files** — they hallucinate file contents. Use `general-purpose` agents for tasks requiring file reads.
- **Always instruct agents to preserve ALL code examples, method names, signatures, return types, property names, and tables EXACTLY** — only rewrite surrounding English prose. Without this, agents will invent fictional API surfaces.
- **docfx build command** (from WSL): `cmd.exe /c "C:\Users\tcox\.dotnet\tools\docfx.exe D:\dev\src\StoryCADAPI\docs\docfx.json"`
- **Custom CSS**: `docs/public/main.css` constrains article width to 900px and fixes landing page layout.
