# Fiction Prompt Generator — design & research docs (StoryCADAPI#11)

## Provenance — read this first
These documents did **not** originate in this repo. They were authored in the
**Collaborator** repo under `devdocs/issue_61_prompt_generator/` while the Prompt
Generator was tracked as **Collaborator#61**.

On **2026-05-05** the tracking issue was **transferred** Collaborator#61 →
**StoryCADAPI#11** ("Fiction Prompt Generator for StoryCAD") via the `transferIssue`
GraphQL mutation, and parented to umbrella **StoryCAD#1246** (Release 4.2) as the first
formal sub-issue under the new sub-issue policy. The Prompt Generator is planned as a
**Blazor WASM** site in *this* repo (not yet built; LLM path not started).

Because the issue moved here but the design docs were left behind in Collaborator,
these files were relocated to this repo on **2026-05-22** to live alongside the issue
they describe. The Collaborator copies were then removed; this repo is now their sole
home.

> **History note:** 3 of these files (`data_inventory.md`, `research.md`,
> `status_log.md`) had git history in Collaborator; the other 6 were never committed
> there and exist only as relocated here. References to "Collaborator#61", "API-Samples
> repo", and "issue #61" inside these docs predate the transfer — treat them as
> historical; the current home is **StoryCADAPI#11** in this repo.

## Contents
- `status_log.md` — session log / decisions (Phase: Research/Design)
- `research.md` — research notes + business context (Getting Attention lead-gen driver)
- `data_inventory.md` — StoryCAD data catalog usable for generation
- `interaction_design.md` — data → UI interaction patterns
- `broad_story_prompt.md`, `story_element_prompt.md` — prompt schema definitions
- `blazor_github_pages_deployment.md` — Blazor WASM → GitHub Pages deploy research
- `perplexity_prompt.md` — reusable universal LLM prompt template
- `prompt_generator_samples.md` — integration notes
