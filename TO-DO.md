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

## [DONE] BUG: Changing the Size of a Page

Changing the size of the page does not "stick" The page reverts to its default.

Fixed. Saving a page stored its elements but never its size, so the next time the page was shown it went back to its layout's or the project's size. Now the Width and Height you give the page (the canvas in Properties, with nothing selected) are saved with the page, and with a layout too. A page whose size you leave alone still stores none of its own, so it keeps following its layout's or the project's size.

## [DONE] Outline Highlight

There should be a highlight in the Outline when an object is selected in the Designer.

Done. Every element selected in the Designer is highlighted in the Outline, several at once when you select several. The highlight is a soft band behind the entry in the theme's text color, so it shows in Dark, Light and Classic. It follows the selection as it changes, and stays when the Outline is rebuilt (after drawing, deleting, grouping). Clicking an entry in the Outline still selects that element in the Designer. The Outline does not scroll to a highlighted entry that is out of view.


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

## [QUESTION] Text Component/Tool

Make the `Text` component (Tool) transparent background by default, text centered horizontally and vertically in it. 

Explain: How to accomplish the alignment of text within its frame? My preference would be to use the same 9-point chooser as PDFTools is using. Register it as property editor for TextAlignment fields in the PropertyGrid.

Done. A new Text element is transparent and its text is centered, across and down. Its whole frame can still be clicked to select it.

**How the alignment within the frame works**

- **Across:** a text is a WPF `TextBlock` as wide as its frame, so `TextAlignment` (Left, Center, Right) already places its lines across it.
- **Down:** `TextBlock` has no vertical alignment of its own, and it seals the methods that measure and draw it, so nothing can be drawn differently. Instead, the text gets `TextVerticalAlignment` (Top, Center, Bottom). Centered or at the bottom, the text measures its lines with its own font and wrapping, and moves them down by setting its top padding to the room they leave: half of it when centered, all of it at the bottom. It does this again whenever the text, the font, the wrapping or the frame's size changes.
- **Saving:** it is saved in the text's `VerticalAlignment`, which was saved before but never used. Earlier texts have `Stretch` there, which counts as Top, so they look as they did.
- **The chooser:** it is the 9-point chooser of PDFTools, MaxServices.Core's `PagePositionControl`, from the package the Designer already uses. It is registered as the property editor of `TextAlignment` for texts and boxes, and shows in that row with its label ("Center", "Top-Left" …).
  - For a text, it sets `TextAlignment` and `TextVerticalAlignment`.
  - For a box, it sets `TextAlignment`, `TextHorizontalAlignment` and `TextVerticalAlignment`.
  - Justify shows as left. Other `TextAlignment` properties keep their drop-down list.
- **Background:** a text's background is still not restored from the file, as before; it shows transparent. The sample projects have texts saved as White (`optimax-old.wf`) and as the old tool's gray (`WireFlow.wf`); showing them would change how those pages look.

**Should texts show the background saved for them?**

## [DONE] Tools|Options

Create an Options dialog like MarkdownTools have. The first option to set (and implement) is "Reopen previous project on startup."

Done. The Designer has a new **Tools** menu with **Options…**. The dialog is the one MaxPad, MaxWrite and the PDF Workshop use, from the same package (`OptimaxTools.Options` 1.0.0, copied into WireFlow's `LocalPackages` as MaxServices.Core is), so it looks the same: groups on the left, the chosen group's options on the right, OK and Cancel, in the Designer's theme.

- **Files → Reopen the previous project at startup** (off until you turn it on). The project open when you close the Designer opens again, on its home page, the next time it starts. If you closed the project before quitting, nothing reopens; a project that has been moved or deleted is skipped without a message.
- The setting is kept in the Designer's settings file (`%APPDATA%\WireFlow\WireFlow.Designer.config`), as `ReopenPreviousProject`, with the project in `PreviousProject`.
- There is no "Edit Settings File" button: the Designer rewrites that file as it closes, so edits made to it while the Designer runs would be lost.

Adding further options is now a line or two each (see "Options" in `implementation.md`). The theme is still in View → Theme only; say if you want it in Options as well, as the PDF Workshop has it.

## [DONE] Create Tool Glyphs

Create vector glyphs (first as SVGs, stored as authoritative source) to be displayed in front of each tool/component in Tools list.

Done. Every tool in the Tools list has a glyph in front of its title: Select (the mouse pointer), Pan (four arrows), Navigate (a compass), Rectangle, Ellipse, Text (a capital T), Box (a frame holding lines of text), Image (hills under the sun) and Stack Panel (a frame divided into a stack).

- **Source:** the SVGs in `WireFlow.Designer\Glyphs`, one per tool. They are line glyphs on a 24 x 24 grid, drawn as PDFTools' toolbar icons are: round strokes 1.7 wide, no fill, in `currentColor`.
- **XAML:** as you asked, they were converted once into `WireFlow.Designer\Xaml\ToolGlyphs.xaml`, and the Designer uses that alone; it reads no SVG. The converter, `Documents\make-tool-glyphs.cs`, stays beside `make-icons.ps1`. To change a glyph or add one, edit or add the SVG and run `dotnet run Documents\make-tool-glyphs.cs` from the repository root; it checks that WPF reads each glyph and that it stays on its grid.
- **Look:** each glyph is drawn 18 px square in its row's own text color, so it is white in Dark, black in Light and Classic, and follows a selected row.

# END-OF-TO-DO