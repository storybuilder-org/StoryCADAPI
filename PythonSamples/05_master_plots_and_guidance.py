"""Explore StoryCAD's read-only writing-guidance resources.

Demonstrates: master plots (names / notes / scene breakdowns), stock scenes by
category, key questions per element type, and per-property examples. These are
reference data you can surface to a user while they fill out an outline.
"""
from storycad import StoryCAD

sc = StoryCAD(headless=True)
sc.create_empty_outline("Guidance Demo", "Jake", template_index="2")

# Master plots: a named arc with notes and a scene-by-scene breakdown.
plots = sc.get_master_plot_names()
print(f"{len(plots)} master plots. First few: {plots[:4]}")

plot = plots[0]
print(f"\nNotes for '{plot}':\n  {sc.get_master_plot_notes(plot)[:120]}...")
print(f"\nScenes in '{plot}':")
for scene in sc.get_master_plot_scenes(plot)[:3]:
    print(f"  - {scene.title}: {scene.notes[:50]}...")

# Stock scenes, organised by category.
categories = sc.get_stock_scene_categories()
print(f"\nStock-scene categories: {categories[:4]}")
print(f"Scenes in '{categories[0]}': {sc.get_stock_scenes(categories[0])[:3]}")

# Key questions help a writer interrogate each element type.
print("\nKey-question element types:", sc.get_key_question_elements())
for q in sc.get_key_questions("Character")[:2]:
    print(f"  [{q.topic}] {q.question[:70]}...")

# Examples for an individual property (handy for autocompletion UIs).
print("\nExample 'Role' values:", sc.get_examples("Role")[:5])
