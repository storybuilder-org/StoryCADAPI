"""Create scenes, populate their casts, and place a setting.

Demonstrates: building Setting and Scene elements, add_cast_member to put
characters on stage, and move_element to reorder nodes in the tree.
"""
from storycad import StoryCAD

sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("Heist", "Jake", template_index="2")[0]

vault = sc.add_element(sc.item_type.Setting, overview, "The Vault")
hero = sc.add_element(sc.item_type.Character, overview, "Mara")
accomplice = sc.add_element(sc.item_type.Character, overview, "Dex")

# Three scenes of a sequence.
casing = sc.add_element(sc.item_type.Scene, overview, "Casing the Vault")
breakin = sc.add_element(sc.item_type.Scene, overview, "The Break-In")
escape = sc.add_element(sc.item_type.Scene, overview, "The Escape")

# Put characters on stage in each scene.
for scene in (casing, breakin, escape):
    sc.add_cast_member(scene, hero)
    sc.add_cast_member(scene, accomplice)

# Tie the climactic scene to its setting. The Scene's "Setting" property is a
# reference, so it takes the Setting element's GUID (as a string), not its name.
sc.update_element_property(breakin, "Setting", str(vault))

# Reorder: move the escape scene to sit between casing and the break-in.
# overview's children at this point are vault, hero, accomplice, casing,
# break-in, escape; index 4 lands escape right after casing.
sc.move_element(escape, overview, index=4)

print("Scenes:")
for s in sc.get_elements_by_type(sc.item_type.Scene):
    print(f"  - {s.name}")

sc.write_outline("/tmp/heist.stbx")
print("\nSaved to /tmp/heist.stbx")
