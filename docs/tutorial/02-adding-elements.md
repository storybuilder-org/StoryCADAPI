---
layout: default
title: "2. Adding Story Elements"
parent: "Tutorial"
nav_order: 2
---

# 2. Adding Story Elements
{: .no_toc }

An empty outline isn't much of a story. In this lesson you'll add the building blocks (characters, a setting, and a few scenes) and then read them back to check your work.

1. TOC
{:toc}

## Elements and the tree

Everything in an outline is a **story element**: a character, a scene, a setting, a problem, and so on. Elements are arranged in a tree, and at the top sits the Story Overview you met in Lesson 1. When you add an element, you tell StoryCAD what *kind* it is and which element it belongs under.

We'll keep using the outline from last lesson, so start the same way and create it:

<div class="code-tabs" markdown="1">

```csharp
BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

var result = await api.CreateEmptyOutline("The Vault", "Jane Doe", "0");
var overview = result.Payload[0];
var root = overview.ToString();   // the parent GUID, as a string
```

```python
sc = StoryCAD(headless=True)

overview = sc.create_empty_outline("The Vault", "Jane Doe", template_index="0")[0]
```

</div>

In C#, `AddElement` takes the parent as a GUID string, so we keep `root` handy. In Python you just pass the `overview` element straight in.

## Add the cast

Let's introduce our three characters. Each call adds one `Character` under the overview and gives you back a handle to it.

<div class="code-tabs" markdown="1">

```csharp
var mara   = api.AddElement(StoryItemType.Character, root, "Mara Vance").Payload;
var eli    = api.AddElement(StoryItemType.Character, root, "Eli").Payload;
var broker = api.AddElement(StoryItemType.Character, root, "The Broker").Payload;
```

```python
mara   = sc.add_element(sc.item_type.Character, overview, "Mara Vance")
eli    = sc.add_element(sc.item_type.Character, overview, "Eli")
broker = sc.add_element(sc.item_type.Character, overview, "The Broker")
```

</div>

Notice the type comes from `StoryItemType` in C# and from `sc.item_type` in Python. Besides `Character`, you have `Setting`, `Scene`, `Problem`, `Folder`, `Section`, `Web`, and `Notes`: the [Element Types](../concepts/element-types.md) page lists them all.

## Add a setting and some scenes

A heist needs a place to rob and a sequence of scenes. Same call, different types:

<div class="code-tabs" markdown="1">

```csharp
var vault = api.AddElement(StoryItemType.Setting, root, "The Vault").Payload;

var casing  = api.AddElement(StoryItemType.Scene, root, "Casing the Job").Payload;
var breakIn = api.AddElement(StoryItemType.Scene, root, "The Break-In").Payload;
var getaway = api.AddElement(StoryItemType.Scene, root, "The Getaway").Payload;
```

```python
vault = sc.add_element(sc.item_type.Setting, overview, "The Vault")

casing   = sc.add_element(sc.item_type.Scene, overview, "Casing the Job")
break_in = sc.add_element(sc.item_type.Scene, overview, "The Break-In")
getaway  = sc.add_element(sc.item_type.Scene, overview, "The Getaway")
```

</div>

Your outline now holds three characters, one setting, and three scenes, all sitting directly under the overview.

## Read them back

Let's confirm what we've got. `GetElementsByType` returns every element of one kind, so we can list the cast:

<div class="code-tabs" markdown="1">

```csharp
var characters = api.GetElementsByType(StoryItemType.Character).Payload;
Console.WriteLine($"Cast ({characters.Count}):");
foreach (var character in characters)
{
    Console.WriteLine($"  - {character.Name}");
}
```

```python
characters = list(sc.get_elements_by_type(sc.item_type.Character))
print(f"Cast ({len(characters)}):")
for character in characters:
    print(f"  - {character.name}")
```

</div>

Run it and you'll see your three characters:

```text
Cast (3):
  - Mara Vance
  - Eli
  - The Broker
```

Save the outline before you stop, so your work is on disk for the next lesson:

<div class="code-tabs" markdown="1">

```csharp
await api.WriteOutline("the-vault.stbx");
```

```python
sc.write_outline("the-vault.stbx")
```

</div>

## What you learned

- `AddElement` creates an element of a given `StoryItemType` under a parent, and hands you a reference to it.
- The Story Overview is the parent for top-level elements.
- `GetElementsByType` lists every element of one kind, handy for checking your work.

Your cast exists, but right now they're just names. Next we'll give them roles, ages, and notes.

**Next:** [Reading and Editing Elements](03-reading-and-editing.md) →
