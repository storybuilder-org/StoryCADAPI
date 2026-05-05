# StoryCAD Outliner Engine

This module converts narrative prose into a structured StoryCAD outline using a pass-based interaction with an LLM.

---

## 🧠 Core Concepts

* **Pass-based orchestration**: The outlining process occurs in four logical passes — Characters, Settings, Problems, Scenes — each informing the next.
* **Full-text initialization**: The entire story is sent once during the first pass.
* **Model retains prose context**: The LLM holds the full prose across passes.
* **Host assigns GUIDs**: New elements receive GUIDs from the host and are sent back via `guid_patch`.

---

## 🗂 Files Overview

### ⚙️ C# Support

* `ContentPageViewModel.cs`: Orchestrates the outlining flow.
* `ModelResponse.cs`: Deserializes the model's output.
* `GuidPatch.cs`: Tracks name-to-guid assignments across passes.
* `PassType.cs`: Enum for identifying each structural pass.

### 📘 Prompts and Heuristics

* `StoryCAD-SystemPrompt.md`: Defines agent role, behavior, and I/O contract.
* `OutliningHeuristics.md`: Lists extraction rules and structural expectations.

---

## ✅ How It Works

1. **User opens a file** → `.txt`, `.docx`, or `.pdf`.
2. **System prompt is assembled** → `SystemPrompt + Heuristics`.
3. **Characters pass sends the full prose** along with the prompt.
4. **Each subsequent pass** sends only the pass name and `guid_patch`.
5. **Model returns `updated_elements`** for each pass.
6. **Host adds or updates elements** via StoryCADLib API.

---

## 🔧 Requirements

* The full prompt and prose must fit within the model’s context window
* A long-context LLM (e.g., GPT-4o, Claude Opus) is required
* Markdown files (`.md`) must reside in the app’s working directory

---

## 🧪 Testing Tips

* Use short stories for early testing
* Verify one pass at a time via `RunPassAsync(PassType.X)`
* Inspect the `guidMap` between passes to ensure continuity

---

## 📦 TODO

* Support automatic StoryOverview update
* Add interactive error resolution or revision capability
* Optionally persist full outline state between sessions
