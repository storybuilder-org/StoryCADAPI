---
layout: default
title: "Tutorial"
nav_order: 3
has_children: true
---

# The StoryCAD API Course

Welcome. This is a hands-on course in building story outlines with the StoryCAD API. You'll start with the simplest possible program — create an outline and save it — and add one layer at a time until you've used the whole API.

Every step is small. Work through them in order, type the code yourself, and run it. By the end you'll have built a complete outline for a short heist story, *The Vault*, and you'll know how to drive every part of the library.

## Before you start

You'll need the library installed and a project to run code in. The [Installation](../getting-started/installation.md) page covers that for both C# and Python. If you've already done the [Quick Start](../getting-started/quick-start.md), you're ready — this course picks up where it leaves off and goes deeper.

Every example comes in two languages. Use the **C#** / **Python** tabs on each code block to follow along in whichever you prefer:

<div class="code-tabs" markdown="1">

```csharp
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

```python
sc = StoryCAD(headless=True)
```

</div>

## What you'll build

One outline, grown across the lessons:

> *The Vault* — Mara Vance, a retired thief, is pulled back for one last job by her old mentor Eli. Standing in her way is the man who set her up years ago: the Broker.

You'll add the cast, place the scenes, wire up relationships, give the story a structured problem and a beat sheet, then search, tidy, and save it.

## The lessons

1. [Your First Outline](01-first-outline.md) — create an outline, save it, open it again.
2. [Adding Story Elements](02-adding-elements.md) — characters, settings, and scenes; the element tree.
3. [Reading and Editing Elements](03-reading-and-editing.md) — set properties and inspect what you've stored.

More lessons are on the way, following this path:

{: .text-grey-dk-000 }
4. Organizing the Outline — folders, sections, and moving elements
5. Cast and Relationships — who's in each scene, and how they're connected
6. Problems and Conflict — give the story a spine using the conflict catalog
7. Structure with Beat Sheets — apply and edit a story-structure template
8. The Writing Tools — master plots, stock scenes, and key questions
9. Searching Your Outline — find elements and clean up references
10. Deleting, Trash, and Recovery — remove elements safely
11. Putting It Together — the full script, end to end

## How this fits with the rest of the docs

This course teaches the API by building something. When you want a quick lookup instead of a walkthrough, the [API Reference](../api/index.md) lists every method, and [Operations](../operations/index.md) covers individual topics in isolation. The [Samples](../samples/index.md) are complete programs you can read and run.

Ready? Start with [Your First Outline](01-first-outline.md).
