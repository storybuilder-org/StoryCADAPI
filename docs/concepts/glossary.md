---
layout: default
title: "Glossary"
parent: "Concepts"
nav_order: 4
---

# Glossary

One-line definitions of the StoryCAD terms used across this documentation. Each links to the page that covers it in full, or to the [StoryCAD user manual](https://manual.storybuilder.org/) for concepts that belong to the application rather than the API.

## Beat sheet

A story-structure framework, such as Three Act Structure, Hero's Journey, or Save the Cat, that defines an ordered list of narrative beats. You apply a beat sheet to a Problem element and assign Scenes to its beats. See [Beat Sheet Operations](../operations/beat-sheets.md).

## Dramatic situation

A writing-craft way of classifying the core conflict of a story or scene, drawn from classic catalogs such as Georges Polti's *36 Dramatic Situations*. It is a planning aid in the StoryCAD application, not a resource the API exposes; the API's structural tools are beat sheets and master plots. See the [StoryCAD user manual](https://manual.storybuilder.org/).

## Element type

The kind of a story element, given by the `StoryItemType` enum. There are eleven: StoryOverview, Problem, Character, Setting, Scene, Folder, Section, Web, Notes, TrashCan, and StoryWorld. See [Element Types and Properties](element-types.md).

## Headless mode

Running StoryCADLib without its user interface, for console apps, web APIs, and batch tools. Enable it with `BootStrapper.Initialise(headless: true)` in C#, or by constructing `StoryCAD(headless=True)` in Python. See [Quick Start](../getting-started/quick-start.md).

## Master plot

One of the twenty classic plot patterns from Ronald Tobias's *20 Master Plots*, such as Quest, Pursuit, or Revenge. Each carries descriptive notes and a scene breakdown, exposed as read-only reference data through `GetMasterPlotNames`. See [Master Plots](resource-data.md#master-plots).

## OperationResult

The wrapper every C# API method returns. Check `IsSuccess` before reading `Payload`, and read `ErrorMessage` on failure. The Python wrapper takes the opposite approach: it returns values directly and raises an exception on error. See [OperationResult](../api/operation-result.md).

## `.stbx` file

StoryCAD's outline file format: a JSON document holding every story element and the flattened tree views. Written with `WriteOutline` and read with `OpenOutline`. See [File Format](story-model.md#file-format).

## Story element

A single node in an outline, such as a character, scene, problem, or setting. Every story element is a `StoryElement` subclass identified by a GUID, and elements reference each other by GUID rather than by object reference. See [Element Types and Properties](element-types.md).

## StoryModel

The root data structure for an outline. It holds a flat collection of all story elements plus three tree views: ExplorerView for authoring, NarratorView for reading order, and TrashView for deleted elements. Every API call reads from or writes to the StoryModel. See [The Story Model](story-model.md).
