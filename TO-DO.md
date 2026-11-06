---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] BUG: Page Multiple Times

The same page can be opened multiple times, in separate tabs. Selecting a page that's already open should switch to that tab.

Fixed. Opening a page, layout or part that is already open switches to its tab instead of opening a second one. That covers a double-click in Pages, Layouts or Parts, a click in Pages in Navigate mode, and a link followed in Navigate mode; the tab you were in stays on its own page. Names compare ignoring case. A page and a layout with the same name are still told apart. A tab dragged out into a floating window is not found, as before.

## BUG: Changing the Size of a Page

Changing the size of the page does not "stick" The page reverts to its default.

## Outline Highlight

There should be a highlight in the Outline when an object is selected in the Designer.


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

## [DONE] Remove Dependency on Wpf.Toolkit

Implement property grid as `OptimaxTools.Wpf.PropertyGrid`. Remove the dependency on Extended.Wpf.Toolkit. 

Done. `OptimaxTools.Wpf.PropertyGrid` is a new project in the solution, and Extended.Wpf.Toolkit is gone from the Designer and the third-party notices. The grid keeps what the Designer used of Xceed's: categories or alphabetical order, the filter box, the header with the object's type and name, a name column you can drag wider, and editors chosen by type:

- check boxes for Booleans;
- lists for enums, font weights, styles and stretches, and font families;
- a color swatch with text for brushes and colors;
- text through each property's converter for everything else, so `Auto`, `1,2,3,4` and `#FF0000` work as in XAML.

Its rows are compact in every theme, and an invalid value shows a red frame with the reason in a tooltip. `PropertyValueChanged` now fires only for an edit made in the grid, so the old "fires on every mouse click" problem is gone. Editors are chosen through `PropertyEditors.Register`, as with Delphi's `RegisterPropertyEditor`, which "Extend PropertyGrid" builds on. The summary pane at the bottom of Xceed's grid is not there; a property's description shows as the tooltip of its name.

## [DONE] Extend PropertyGrid

Extend PropertyGrid so that custom property editors can be registered, as in Delphi Object Inspector. In the Designer, wire up any color properties to the color picker from Optimax shared library.

The custom editors would be invoked by a button at the right end of the property value, like in Delphi.

Done. A `PropertyEditor` registered through `PropertyEditors.Register` can now have a dialog (`HasDialog` and `ShowDialog`, like Delphi's `paDialog`). Its row then shows a "…" button at the right end of the value, and Ctrl+Enter in the value does the same. In the Designer every brush and color property (Background, Foreground, BorderBrush and so on) opens the color dialog; OK sets a solid brush of the chosen color.

The Optimax shared library (MaxServices.Core 1.1.0) has no color picker of its own. The dialog is the one MarkdownTools (MaxPad) and PDFTools use, built on PixiEditor.ColorPicker 3.4.2.3 (MIT), copied into the Designer as `Views\ColorPickerDialog.cs`. It has the square of shades, the RGB, HSV and HSL sliders with alpha, the opacity in percent, and the web color as `#AARRGGBB`. Its license and those of its two dependencies are in `THIRD-PARTY-NOTICES.md`. If you'd rather have the dialog in Optimax.Shared.Lib, so that all the programs share one copy, it can move there as it is.

## [DONE] StackPanel

Create a `StackPanel` container component to be listed in Tools. It would align its children either vertically, or horizontally, depending on a `Direction` property setting (`Vertical`, `Horizontal`) and stretch them in the direction orthogonal to its `Direction`. Just like a stack panel would.

Done. Tools has a new **Stack Panel** tool, after a separator below Image.

- **Drawing:** you draw the panel as you would a box. A new panel is a faint translucent gray, so that an empty one shows; change its Background in Properties.
- **Adding children:** draw an element *starting inside* the panel, and it becomes the panel's last child. Nested panels work: the innermost one under the starting point takes the element.
- **Layout:** each child keeps its own size in the panel's `Direction` (Properties, `Vertical` by default) and is stretched across the panel in the other direction. Children that don't fit are clipped.
- **Editing:** a click on a child selects the panel, as with a group. Ungroup releases the children where they are.
- **Saving:** a panel is saved as a new element kind, `stack`, with its `Direction`; the Browser shows it as well. The file format stays 2.
- **Changing direction:** each child's size along the new direction is the stretched size it had. For example, a vertical panel's children are as wide as the panel when it turns horizontal.

Assumption: "stack the selection" (turning selected elements into a panel, as Group does) is not there; say if you want it.

## Text Component/Tool

Make the `Text` component (Tool) transparent background by default, text centered horizontally and vertically in it. 

Explain: How to accomplish the alignment of text within its frame? My preference would be to use the same 9-point chooser as PDFTools is using. Register it as property editor for TextAlignment fields in the PropertyGrid.

## Tools|Options

Create an Options dialog like MarkdownTools have. The first option to set (and implement) is "Reopen previous project on startup."

## Create Tool Glyphs

Create vector glyphs (first as SVGs, stored as authoritative source) to be displayed in front of each tool/component in Tools list.

# END-OF-TO-DO