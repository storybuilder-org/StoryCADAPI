"""Define a Problem and drive it with StoryCAD's conflict resources.

Demonstrates: the conflict catalog (categories -> subcategories -> examples),
and applying conflict text to the protagonist and antagonist of a Problem.
"""
from storycad import StoryCAD

sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("Conflict Demo", "Jake", template_index="2")[0]

problem = sc.add_element(sc.item_type.Problem, overview, "Central Problem")

# Browse the built-in conflict catalog.
categories = sc.get_conflict_categories()
print(f"{len(categories)} conflict categories available, e.g.: {categories[:5]}")

category = categories[0]
subcategories = sc.get_conflict_subcategories(category)
print(f"\nSubcategories of '{category}': {subcategories[:5]}")

if subcategories:
    examples = sc.get_conflict_examples(category, subcategories[0])
    print(f"Examples for '{subcategories[0]}': {examples[:3]}")

# Apply opposing conflicts to the two sides of the problem.
sc.apply_conflict_to_protagonist(problem, "Wants to clear her family's name.")
sc.apply_conflict_to_antagonist(problem, "Wants the secret buried forever.")

sc.write_outline("/tmp/conflict_demo.stbx")
print("\nSaved to /tmp/conflict_demo.stbx")
