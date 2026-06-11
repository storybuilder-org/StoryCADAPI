---
layout: default
title: "3. Reading and Editing Elements"
parent: "Tutorial"
nav_order: 3
---

# 3. Reading and Editing Elements
{: .no_toc }

Your characters have names, but a name isn't a character. In this lesson you'll set their properties (role, age, a note or two) both one at a time and in batches, then read an element back to see everything you've stored.

1. TOC
{:toc}

## Properties

Every element has a set of **properties**: fields like `Role`, `Age`, or `Notes` on a character. You set a property by naming it and giving it a value. Let's pick up where Lesson 2 left off, with Mara and the Broker already created.

## Set one property

Start small. Mara is our protagonist, so let's say so:

<div class="code-tabs" markdown="1">

```csharp
api.UpdateElementProperty(mara, "Role", "Protagonist");
```

```python
sc.update_element_property(mara, "Role", "Protagonist")
```

</div>

That's the whole pattern: the element, the property name, the new value. Do the same for our antagonist:

<div class="code-tabs" markdown="1">

```csharp
api.UpdateElementProperty(broker, "Role", "Antagonist");
```

```python
sc.update_element_property(broker, "Role", "Antagonist")
```

</div>

## Set several at once

Setting properties one call at a time gets tedious when you have a few to fill in. `UpdateElementProperties` takes a whole batch:

<div class="code-tabs" markdown="1">

```csharp
var maraDetails = new Dictionary<string, object>
{
    { "Age", "38" },
    { "Notes", "Retired thief. Swore she was done. She isn't." },
};
api.UpdateElementProperties(mara, maraDetails);
```

```python
sc.update_element_properties(mara, {
    "Age": "38",
    "Notes": "Retired thief. Swore she was done. She isn't.",
})
```

</div>

Same idea, just a map of names to values. Use a single update for one field and a batch when you're filling in several.

{: .note }
> **Which property names can I use?** They match the fields StoryCAD knows about for each element type: `Role`, `Age`, `Archetype`, and so on for a character. If you set a name the element doesn't recognize, the call simply won't change anything. The [Element Types](../concepts/element-types.md) page lists the properties for each type.

## Read an element back

To see everything stored on an element, ask for it. `GetElement` returns the element's full state: every field, not just the ones you set.

<div class="code-tabs" markdown="1">

```csharp
var detail = api.GetElement(mara).Payload;
Console.WriteLine(detail);   // the complete element, all fields
```

```python
import json

detail = json.loads(sc.get_element(mara))
print(f"Role:  {detail['Role']}")
print(f"Age:   {detail['Age']}")
print(f"Notes: {detail['Notes']}")
```

</div>

In Python `get_element` returns a JSON string, so `json.loads` turns it into a dictionary you can read field by field. In C# you get the serialized element back as the result's `Payload`. Either way, what you set is what you get:

```text
Role:  Protagonist
Age:   38
Notes: Retired thief. Swore she was done. She isn't.
```

Save your progress:

<div class="code-tabs" markdown="1">

```csharp
await api.WriteOutline("the-vault.stbx");
```

```python
sc.write_outline("the-vault.stbx")
```

</div>

## What you learned

- `UpdateElementProperty` sets one field; `UpdateElementProperties` sets a batch.
- Property names are the fields StoryCAD defines for each element type.
- `GetElement` returns an element's complete stored state: useful for checking what you've saved.

Your cast is taking shape. So far everything sits in a flat list under the overview, though. Next we'll organize the outline into folders and put the scenes in order.

**Next:** Organizing the Outline *(coming soon)*
