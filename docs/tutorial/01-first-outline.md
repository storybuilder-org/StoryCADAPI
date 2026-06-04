---
layout: default
title: "1. Your First Outline"
parent: "Tutorial"
nav_order: 1
---

# 1. Your First Outline
{: .no_toc }

In this first lesson you'll create an outline, save it to a file, and open it back up. That's the whole loop every StoryCAD program is built on, so it's worth getting comfortable with it before we add anything else.

1. TOC
{:toc}

## Start the library

Before you call any method, you start the library in **headless** mode — no windows, no UI, just the API. You do this once, when your program starts.

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Start StoryCADLib once, at startup.
BootStrapper.Initialise(headless: true);

// Grab the API. This is the object you'll use for everything.
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

```python
from storycad import StoryCAD

# Start StoryCADLib in headless mode. The object you get back
# is the API — you'll use it for everything.
sc = StoryCAD(headless=True)
```

</div>

In C# the library uses a dependency-injection container, so you initialize it and then ask the container for the API. In Python that's all wrapped up for you: the `StoryCAD` object you create *is* the API. From here on, the two read almost the same.

## Create an outline

Now make an empty outline. You give it a title, an author, and a *template* — `"0"` is a blank outline, which is exactly what we want to start with.

<div class="code-tabs" markdown="1">

```csharp
var result = await api.CreateEmptyOutline("The Vault", "Jane Doe", "0");
var overview = result.Payload[0];   // the Story Overview — the root of your outline
```

```python
overview = sc.create_empty_outline("The Vault", "Jane Doe", template_index="0")[0]
# the first element back is the Story Overview — the root of your outline
```

</div>

Creating an outline hands you back its elements. The very first one is the **Story Overview** — the root that everything else hangs off. Hold on to it; in the next lesson you'll add characters and scenes underneath it.

{: .note }
> **A quick word on results.** In C#, every API call returns an `OperationResult`. You read the data from its `Payload`, and you can check `IsSuccess` to see whether the call worked. Python skips the wrapper: a call returns its value directly and raises an error if something goes wrong. We'll keep the examples simple and trust the calls to succeed; [the OperationResult page](../api/operation-result.md) shows how to check for errors when you need to.

## Save it

An outline lives in memory until you write it out. StoryCAD files use the `.stbx` extension.

<div class="code-tabs" markdown="1">

```csharp
await api.WriteOutline("the-vault.stbx");
```

```python
sc.write_outline("the-vault.stbx")
```

</div>

That's a real, openable file. If you have the StoryCAD app, open `the-vault.stbx` and you'll see your outline — empty for now, but yours.

## Open it again

Opening a saved outline makes it the one you're working on, so every call after this acts on it.

<div class="code-tabs" markdown="1">

```csharp
await api.OpenOutline("the-vault.stbx");
```

```python
sc.open_outline("the-vault.stbx")
```

</div>

## The whole program

Here's everything together — create, save, reopen:

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

BootStrapper.Initialise(headless: true);
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

var result = await api.CreateEmptyOutline("The Vault", "Jane Doe", "0");
var overview = result.Payload[0];

await api.WriteOutline("the-vault.stbx");
await api.OpenOutline("the-vault.stbx");

Console.WriteLine("Outline saved and reopened.");
```

```python
from storycad import StoryCAD

sc = StoryCAD(headless=True)

overview = sc.create_empty_outline("The Vault", "Jane Doe", template_index="0")[0]

sc.write_outline("the-vault.stbx")
sc.open_outline("the-vault.stbx")

print("Outline saved and reopened.")
```

</div>

## What you learned

- Start the library once in headless mode, then get the API object.
- `CreateEmptyOutline` makes a new outline and returns its elements; the first is the Story Overview.
- `WriteOutline` saves to a `.stbx` file; `OpenOutline` loads one back.

That's the skeleton. Next we'll give it some life.

**Next:** [Adding Story Elements](02-adding-elements.md) →
