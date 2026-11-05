---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Tools Line Spacing

In Tools dialog, the lines are too far apart. Make the display almost as compact as the menus.

Done: the Fluent themes pad each list row by 12 px, which made each Tools row 45 px tall. The Tools rows now have their own smaller padding, which makes them 27 px tall in System, Light and Dark, the height of a menu item. Classic pads rows by only 4,1 px, so there they keep their own 23 px. The theme's look (selection, hover) is kept, and switching themes re-applies it. The rows leave room for the tool glyphs of "Create Tool Glyphs". Checked in the real Designer, off screen: the row heights in Dark, Light and Classic. Not yet checked by hand.

## [DONE] Convert All Projects to WF

Convert all existing project repos under Data to the new .`wf` format.

Done: the five folder projects under `Data` are converted, each into a `.wf` beside its `.project`, as the Designer does when it opens one: `MarketMonitor/MarketMonitor.wf`, `OPTIMAX/Optimax.wf`, `Test/Test.wf`, `Test2/Test2.wf` and `WIREFLOW/WireFlow.wf`. Checked: every document, unsaved draft, archived revision and image in each `.wf` is byte for byte the file it came from, and the project and every page, layout and part read back through `SqliteRepository` the same as through the folder storage. Most of these folders hold little on disk: only `WIREFLOW` has a page (`Main`, with its unsaved edits and one archived project revision), `OPTIMAX` has its 7 images, and `MarketMonitor`, `Test` and `Test2` have their project settings only, so their `.wf` files are empty projects. `MarketMonitor.new`, a project file beside `MarketMonitor.project`, is not read by either storage and was left out. `AVG`, `ClearFunction` and `PULSE` are empty folders, with nothing to convert. `Data/FirstTest.wf`, which I did not make, is left as it is and not committed.

Question 1: `Data/OPTIMAX.zip` holds a fuller OPTIMAX project from 2016 (8 pages, 2 layouts, 7 parts), but in an earlier format still: its project file is `Optimax.wireflow`, and its elements are written as `"Type": "text"` rather than with `$type` tags, so neither storage can load them. Converting it would mean writing a migration from that format to today's element model, which is a task of its own. I left the zip as it is. Should it be migrated?

A1: Yes, migrate it to `optimax-old.wf`, as a one time only shot. Do not worry about compatibility going forward.

Question 2: the folder projects are still there beside their `.wf` files, since the conversion leaves them as they are. Should I remove them from `Data` (the `.project` files and their folders, keeping only the `.wf` files), or keep them until the folder storage is dropped? I am keeping them.

A2: Remove the folders and keep only the `.wf` files.

Done, as answered: `Data/optimax-old.wf` is the 2016 project, migrated once: each element got the `$type` tag of its kind (`box`, `text` and `rect` are `FlowElement`, `group` is `FlowGroup`, `image` is `FlowImage`, `part` is a part reference), `type` became `Type`, the trailing commas are gone, and each part got a `FlowPart` tag at its root, so that it can be read back. All 8 pages, 2 layouts, 7 parts (every element counted) and 7 images are in it, and the Designer shows its home page as the 2016 site, with its layout, parts, images and text. The `.cdr` drawings in the zip are not part of the project and were left out. `Data` now holds only `.wf` files: `MarketMonitor.wf`, `Optimax.wf`, `Test.wf`, `Test2.wf` and `WireFlow.wf` moved up from their folders, `optimax-old.wf`, and your `FirstTest.wf`. The folders (with `drawings/`, `test.json`, `test-page.json`, `test.xaml`, `wpfcolors.png`, `index.png` and `new-page.png`), the three empty ones, and `OPTIMAX.zip` are removed; they remain in the git history before this commit.

## XML vs JSON

Weigh the advantages and disadvantages of converting current JSON data — how the project data is stored inside WF — into XML.

## Remove Dependency on Wpf.Toolkit

Implement property grid as `OptimaxTools.Wpf.PropertyGrid`. Remove the dependency on Extended.Wpf.Toolkit. 

## Extend PropertyGrid

Extend PropertyGrid so that custom property editors can be registered, as in Delphi Object Inspector. In the Designer, wire up any color properties to the color picker from Optimax shared library.

The custom editors would be invoked by a button at the right end of the property value, like in Delphi.

## StackPanel

Create a `StackPanel` container component to be listed in Tools. It would align its children either vertically, or horizontally, depending on a `Direction` property setting (`Vertical`, `Horizontal`) and stretch them in the direction orthogonal to its `Direction`. Just like a stack panel would.

## Text Component/Tool

Make the `Text` component (Tool) transparent background by default, text centered horizontally and vertically in it. 

Explain: How to accomplish the alignment of text within its frame? My preference would be to use the same 9-point chooser as PDFTools is using.

## Tools|Options

Create an Options dialog like MarkdownTools have. The first option to set (and implement) is "Reopen previous project on startup."

## Create Tool Glyphs

Create vector glyphs (first as SVGs, stored as authoritative source) to be displayed in front of each tool/component in Tools list.

# END-OF-TO-DO