"""Apply a beat sheet to a Problem, then edit the beats.

Demonstrates: listing beat-sheet templates, inspecting one, applying it to a
Problem, reading the resulting structure, and the beat CRUD surface
(create / update / move / delete) plus assigning a story element to a beat.
"""
from storycad import StoryCAD

sc = StoryCAD(headless=True)
overview = sc.create_empty_outline("Beats Demo", "Jake", template_index="2")[0]

# What templates ship with StoryCAD?
print("Beat sheets:", sc.get_beat_sheet_names())

# Peek at one before committing to it.
sheet = sc.get_beat_sheet("Save The Cat")
print(f"\n'Save The Cat' has {len(sheet.beats)} beats. First three:")
for beat in sheet.beats[:3]:
    print(f"  - {beat.title}: {beat.description[:60]}...")

# Apply it to a problem and read back the live structure.
problem = sc.add_element(sc.item_type.Problem, overview, "Act Structure")
sc.apply_beat_sheet_to_problem(problem, "Save The Cat")

structure = sc.get_problem_structure(problem)
print(f"\nApplied. Problem now has {len(structure.beats)} beats.")

# Edit the beats: add one, rename it, then link a scene to it.
# create_beat appends, so the new beat's index is the pre-create length.
scene = sc.add_element(sc.item_type.Scene, overview, "Opening Image")
new_beat_idx = len(structure.beats)
sc.create_beat(problem, "Custom Beat", "Something the template missed.")
sc.update_beat(problem, new_beat_idx, "Renamed Beat", "Now with a clearer description.")
sc.assign_element_to_beat(problem, 0, scene)

sc.write_outline("/tmp/beats_demo.stbx")
print("\nSaved to /tmp/beats_demo.stbx")
