# StoryCAD Python Samples

Sample scripts demonstrating the `storycad` Python bindings for StoryCADLib.

> **Note:** These samples depend on the `storycad` package, which is part of the Python bindings feature planned for StoryCAD 4.2. They are not runnable until that feature ships.

## Samples

| File | What it demonstrates |
|------|----------------------|
| `01_build_cast.py` | Add characters, set properties, add relationships |
| `02_scenes_and_cast.py` | Create scenes, populate casts, assign settings |
| `03_problem_and_conflict.py` | Define a Problem, browse conflict catalog, apply conflict text |
| `04_beat_sheets.py` | Apply a beat sheet, read structure, create/update/move beats |
| `05_master_plots_and_guidance.py` | Browse master plots, stock scenes, key questions, examples |
| `06_edit_open_inspect.py` | Round-trip an outline, walk element tree, read element state |

## Requirements

- Python 3.10+
- [PythonNet](https://github.com/pythonnet/pythonnet) 3.x
- `storycad` package (distributed with StoryCAD 4.2)

## Running a sample

```bash
pip install pythonnet storycad
python 01_build_cast.py
```
