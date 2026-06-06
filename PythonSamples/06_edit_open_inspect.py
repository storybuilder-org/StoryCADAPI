"""Round-trip an outline and inspect its elements.

Demonstrates: write_outline / open_outline, walking the full element tree with
get_all_elements, and reading an element's complete stored state as JSON with
get_element.
"""
import json

from storycad import StoryCAD

PATH = "/tmp/roundtrip.stbx"

# --- session 1: create and save ---
sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("Round Trip", "Jake", template_index="2")[0]
keeper = sc.add_element(sc.item_type.Character, overview, "Keeper")
sc.update_element_property(keeper, "Role", "Protagonist")
sc.write_outline(PATH)

# --- session 2: reopen and inspect ---
sc.open_outline(PATH)
print("Reopened. Full element tree:")
for e in sc.get_all_elements():
    print(f"  {e.element_type:14} {e.name}")

# get_element returns the element's complete stored state as a JSON string.
detail = json.loads(sc.get_element(keeper))
print(f"\nKeeper stores {len(detail)} fields; a sample: {sorted(detail)[:8]}")
print(f"Keeper's saved Role: {detail.get('Role')!r}")

print(f"\nLoaded and verified {PATH}")
