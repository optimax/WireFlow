---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Fix JSON

Fix the problems with the current JSON data being saved. Decouple .NET class names from node types. Avoid special attributes, like `$type`. 

Convert all WF files into the new format as one-shot. Do not burden the apps with the ability to read the old.

Done. `.wf` files are now format 2 (`PRAGMA user_version` 2), and the programs open format 2 only:

- No `$type` and no .NET names: an element's own `Type` (`box`, `text`, `rect`, `group`, `part`, `image`) chooses its class when it is read. A kind the reader does not know becomes a plain element, shown as an unknown object, rather than failing the page.
- Enums are written by name (`"Stretch": "Uniform"` rather than `2`), an automatic size as `null` rather than `"NaN"`, and `Type` and `Name` come first in each object.
- Every value is written, defaults included, so a file no longer depends on the defaults in the code.
- A part saved in the Designer now reads back; before, `GetPart` returned null for it.
- All 7 files in `Data`, your `FirstTest.wf` included, were upgraded once by `Documents\upgrade-wf-to-format-2.cs`. It checks that each document reads back the same before it changes a file, and can upgrade any other format 1 file you have: `dotnet run upgrade-wf-to-format-2.cs -- <file.wf> ...`. The programs refuse a format 1 file, saying that it is not a WireFlow project or was made by another version of WireFlow (it said "a later version").
- The format is described under "Project file format" in `implementation.md`.

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