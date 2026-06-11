"""Build a cast of characters and wire up their relationships.

Demonstrates: add_element, update_element_property / update_element_properties,
add_relationship (with a mirrored back-reference), and reading elements back
out by type.
"""
from storycad import StoryCAD

sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("Cast Demo", "Jake", template_index="2")[0]

# Add three characters under the story overview.
hero = sc.add_element(sc.item_type.Character, overview, "Mara Vance")
mentor = sc.add_element(sc.item_type.Character, overview, "Old Eli")
villain = sc.add_element(sc.item_type.Character, overview, "The Broker")

# Set a single property, then a batch of properties at once.
sc.update_element_property(hero, "Role", "Protagonist")
sc.update_element_properties(
    hero,
    {"Age": "29", "Notes": "Reluctant thief with a strict moral code."},
)
sc.update_element_property(villain, "Role", "Antagonist")

# Relationships. mirror=True writes the SAME description in both directions,
# so it only fits symmetric phrasings ("Sworn enemy of", "Trusted ally").
# For asymmetric pairs, add each direction with its own wording.
sc.add_relationship(hero, mentor, "Student of")
sc.add_relationship(mentor, hero, "Mentor to")

sc.add_relationship(hero, villain, "Sworn enemy of", mirror=True)

# Read the cast back out.
print("Characters in this outline:")
for c in sc.get_elements_by_type(sc.item_type.Character):
    print(f"  - {c.name} ({c.uuid})")

sc.write_outline("/tmp/cast_demo.stbx")
print("\nSaved to /tmp/cast_demo.stbx")
