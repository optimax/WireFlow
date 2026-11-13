---
Title: TO-DO
---

# DONE WireFlow

[toc]

## [DONE] Convert to .NET 10

Convert the solution to .NET 10.

Done: all projects are SDK-style `net10.0-windows`. Newtonsoft.Json is upgraded to 13.0.4, and `publish-locally.bat` publishes the 64-bit Designer. Both apps start, and a sample project loads and saves. See `HANDOFF.md`.

Question 1: Newtonsoft.Json is still 8.0.3. It has no .NET Core build, so NuGet uses its .NET Framework DLL (warning NU1701), and it has a known high-severity vulnerability, GHSA-5crp-9r3c-p9vr (warning NU1903). Should I upgrade it to 13.0.4? It is also MIT-licensed, and its package is already in `packages/`.

A: Yes.

Question 2: `TO-DO-INSTRUCTIONS.md` says to run `publish-locally.bat` after each task, but there is no such file in the repository. Where is it? Or should I create one, and if so, what should it publish, and to where?

A: Create one. It should publish 64-bit WireFlow Designer to `c:\Programs\Optimax\WireFlow` in Release mode.

## [DONE] Outline Sync

Adding a new element to a page, for example a rectangle, is not reflected in the Outline view.

Done: the Outline is rebuilt after an element is created, duplicated, deleted, grouped, ungrouped or moved in the z-order. The build succeeds and the Designer starts; drawing a shape and watching the Outline was not tested interactively. See `HANDOFF.md`.

## [DONE] Convert Solution

Convert `.sln` to the modern `.slnx`.

Done: `WireFlow.slnx` replaces `WireFlow.sln`, and `WireFlow.sln.DotSettings` is now `WireFlow.slnx.DotSettings`. `dotnet build` and Visual Studio's MSBuild both build it. Visual Studio still has the old `.sln` open; reopen the solution from `WireFlow.slnx`.

## [DONE] Upgrade AvalonDock

Bump to 3.8.2 to retain the permissive license. 3.8.2 still ships only lib/net40 DLLs, the same as 2.9.0. So an upgrade would be referenced the same way as now, through the existing ExcludeAssets/$(PkgExtended_Wpf_Toolkit) pattern.

The in-repo Xceed.Wpf.AvalonDock.Themes.Metro is a source copy of the 2.9 theme. It may need refreshing from the 3.8.2 sources if its templates fall out of step with the 3.8 controls.

Done so far: the Designer and the theme project reference Extended.Wpf.Toolkit 3.8.2, and the solution builds with no code changes. The theme is refreshed from Xceed's 3.6.0 source, keeping WireFlow's two brush colors. No 3.8.x theme source was ever published under MS-PL, and the 3.8 theme differs from 3.6.0 only by one template moved into a keyed resource. The Designer starts with all its panes. `THIRD-PARTY-NOTICES.md` now holds the license texts.

Question: the 3.8.2 package contradicts itself. Its NuGet metadata declares MS-PL, and its description says "v3.8.2 is equivalent to v3.8.1 with an MS-PL license". But the `LICENSE.txt` inside the package is Xceed's Community License Agreement (non-commercial use only). Do you accept 3.8.2 on the strength of its declared MS-PL license, or should I drop to 3.6.0? 3.6.0 is MS-PL in both its package and its GitHub source, but it lacks the 3.7–3.8.1 fixes. Switching is a one-line change in two project files. I am staying on 3.8.2 until you answer.

A: Drop to 3.6.0. We are getting rid of the dependency anyway (see below).

Done: Extended.Wpf.Toolkit is now 3.6.0 in the Designer and the theme project. It matches the 3.6.0 theme source and is MS-PL in both its package and its source. The solution builds with 0 errors and 0 warnings, and the Designer starts with all its panes. `THIRD-PARTY-NOTICES.md` and `implementation.md` are updated.

## [DONE] Properties Pane Styles

 ![pasted-2](<TO-DO.images/pasted-2.png>)

The Properties pane has a white background in what is a dark Theme which is jarring. 

Done: the property list, the empty area below it, the category headers, the text editors and the filter box now use the pane's dark colors. The toolkit's template hard-codes those light colors, so the Designer overrides them from code when the grid loads (see "Known traps" in `implementation.md`). Checked in off-screen renders of the real Designer window, with and without a selected element; the checkbox and the scrollbar keep the standard WPF look, as elsewhere in the Designer. The Fluent themes item below will revisit these colors.

## [DONE] BUG: Page No Longer Shows

The background of a page is no longer distinct from the overall application background and page boundaries cannot be seen.

Done: the cause was the Dark theme. The area around the page became the palette's preview color, `#333333`, which is the background of the sample pages (`#333`), and nothing drew the page's edge; before the themes that area was `#222222`. The Designer and the Browser now draw a 1 px frame just outside the page in the palette's page-frame color, as PDFTools frames its pages (`#5A5A5A` in Dark, `#B8BCC2` in Light), so the page's boundary shows whatever its color and the theme's. The frame follows the page as it is zoomed and panned. The area around the page keeps the theme's preview color, as in PDFTools. Checked in off-screen renders of the real Designer in Dark and Light and of the Browser in Dark. Not yet checked by hand: zooming and panning.

## [DONE] Splitter Styles

Make the splitters more distinct by giving them visible (if understated) borders.

Done: the splitters between the Designer's panes are now a rail with a 1 px border on each side, inside their 5 px grab area, in the color PDFTools uses for its pane grips (`ChromeBorder`: dark gray on dark, light gray on light), so they follow the theme. They had no visible style before because OptimaxTools.Dock, as upstream AvalonDock, cleared the theme's splitter style; that is fixed. Checked in off-screen renders of the real Designer in Dark and Light. Not yet checked by hand: dragging the splitters.

## [DONE] Convert to WPF Fluent Themes

Convert the application chrome to WPF Fluent themes, with the selection of Light, Dark, System, and None/Classic (like PDFTools and other Optimax apps). Make the theme selection available under View|Theme menu (place View after Edit).

Question 1: PDFTools' themes come from your `MaxServices.Core` library (`AppTheme`, `ThemeChoice`, `Palette`, `FluentStyles.xaml`; Optimax.Shared.Lib). To make WireFlow behave like PDFTools, I am copying `MaxServices.Core.1.1.0.nupkg` from PDFTools' `LocalPackages` into a new `LocalPackages\` folder here, with a `nuget.config`, as PDFTools does, rather than writing a second theme implementation. The package has no dependencies of its own. Unless you say otherwise, I proceed this way.

A: Yes.

Question 2: the Designer's current dark look becomes its Dark theme, and the default choice is System, as in PDFTools, so on a light Windows the Designer will now start light. The page and the parts drawn on it keep their own colors; the gray area around the page follows the theme. The choice is kept in the Designer's existing settings (`ConfigSettings`). Unless you say otherwise, I proceed this way.

A: Yes.

Question 3: does the Browser get the same themes? It uses the Infragistics MetroDark theme today, and this item says "the application chrome". I am converting the Designer first and leaving the Browser as it is until you answer.

A: Yes, the same as Designer.

Done: the View menu of the Designer (after Edit) and of the Browser has Theme → System, Light, Dark, Classic, as in PDFTools, and each program remembers its choice (the Browser in a new `%APPDATA%\WireFlow\WireFlow.Browser.config`). Menus, toolbars, panes, pane titles, the Outline, the Properties pane, the docking chrome and the area around the page follow the theme; the page keeps its own colors. The Browser no longer uses the Infragistics MetroDark theme, and its package is removed. Checked in off-screen renders of both real windows in Dark, Light and Classic. Not yet checked by hand, and AvalonDock's Ctrl+Tab window stays white.

## [DONE] Convert Data to SQLite

Convert the WireFlow project files and subordinate folders into SQLite databases, with `.wf` extension. Figure out appropriate schema to support the functionality. Convert both Designer and Browser to use `.wf` files.

Plan: a new `WireFlow.Repository.Sqlite` project implements the same `IFlowRepository` as the folder storage, so the rest of both programs is unchanged. A `.wf` file holds:

- `Project`: the project's settings (size, background, home page) as the same JSON as the `.project` file, and its revision number.
- `Documents`: one row per page, layout and part (kind, name), with its saved JSON and, while it has unsaved edits, their JSON (what the `.new` files hold today).
- `Archive`: earlier revisions of the project, pages, layouts and parts (kind, name, revision, JSON), as the `*.archive` folders hold them.
- `Images`: one row per image (name, bytes).

Each document stays one JSON value, as written today, rather than a table per element type: the element model is open-ended (`$type` tags), and the programs read and write whole pages. The file carries a format number (`PRAGMA user_version`), so that a later format can be recognized.

Question 1: the SQLite library is `Microsoft.Data.Sqlite` (MIT, Microsoft), with the `SQLitePCLRaw` native bundle it depends on (Apache 2.0; SQLite itself is in the public domain). I am using 9.0.2, the version already in the NuGet cache here, and adding their license texts to `THIRD-PARTY-NOTICES.md`. Unless you say otherwise, I proceed this way.

A: OK

Question 2: opening a `.project` in the Designer converts it once into a `.wf` file of the same name beside it (`WireFlow.project` → `WireFlow.wf`) and opens that; the folder is left as it is. New projects are `.wf` files. Unless you say otherwise, I proceed this way.

A: OK. Eventually, we drop the old service.

Question 3: the Browser opens `.wf` files only, since it only views; a `.project` is converted by opening it once in the Designer. Should the Browser convert `.project` files too? I am assuming not.

A: NO, the browser uses the new paradigm only.

Update to question 1: 9.0.2 brings `SQLitePCLRaw.lib.e_sqlite3` 2.1.10, which NuGet flags with a high-severity advisory (NU1903, GHSA-2m69-gcr7-jv3q). I used `Microsoft.Data.Sqlite` 10.0.14 instead, the current .NET 10 release, which brings SQLitePCLRaw 2.1.13 and is not flagged. `THIRD-PARTY-NOTICES.md` has the license texts of both, and a note on SQLite itself.

Done, under the assumptions above: a new `WireFlow.Repository.Sqlite` project keeps a project in one `.wf` file with the tables of the plan, and both programs use it. The Designer's File → New makes `.wf` files, and File → Open also offers `.project` files, which it converts once into a `.wf` beside them, copying every file's JSON as it is, and leaves the folder as it is. If that `.wf` already exists, it opens it instead. The Browser opens `.wf` files and explains that a `.project` has to be opened in the Designer first. Saving, unsaved edits, archived revisions, images, renaming and deleting work as in the folder storage; deleting a part now deletes the part (it used to delete a layout of the same name). Checked: 34 checks of the storage against copies of `Data/WIREFLOW` and `Data/Test`, and off-screen renders of the real Designer converting and showing `Data/WIREFLOW` and of the Browser showing the resulting `.wf`. Not yet checked by hand. Found in passing, in both storages: a part saved in the Designer cannot be read back (`GetPart` returns null), recorded in `implementation.md`; not fixed here.

Questions answered.

## [DONE] Remove Dependency on AvalonDock

Create an equivalent OptimaxTools.Dock assembly using the 3.8.2 AvalonDock code to develop the same functionality. Fix any problems with AvalonDock code in the process. For now, put OptimaxTools.Dock into WireFlow solution. 

Question: Xceed never published the 3.8.2 source code. 3.8.2 exists only as MS-PL binaries on NuGet. On GitHub, the last source under MS-PL is the "V3.6.0 Released" commit (September 2019). From the "V3.7.0" commit on, the repository's `license.md` is the Xceed Community License (non-commercial), although the file headers still say Ms-PL. Which source should OptimaxTools.Dock start from?
(a) The MS-PL 3.6.0 source. It is clean licensing, but it lacks the 3.7–3.8.1 fixes; I would re-create any we need, comparing behaviour against the 3.8.2 binaries.
(b) A decompilation of the MS-PL 3.8.2 binaries. MS-PL allows derivative works, but the result has no comments, and the XAML has to be recovered from BAML.
(c) Dirkster.AvalonDock. It is an MS-PL fork of Xceed AvalonDock, maintained since 2019, with .NET Core/.NET 8+ builds and many fixes. It is not "the 3.8.2 code", though.
Unless you say otherwise, I will assume (a) when I reach this item.

A: Let's explore the Dirkster option. What license does it have?

Answer: MS-PL, the same license as Xceed AvalonDock 3.6.0 and the one `THIRD-PARTY-NOTICES.md` already carries. It is MS-PL in both places: the repository's `LICENSE` file and the `LICENSE` bundled in its NuGet packages. Dirkster99 forked Xceed's MS-PL source before Xceed switched to the Community License, so the fork never took on the non-commercial terms. It is actively maintained: v5.0.1 was released on 2026-10-04 and targets `net10.0-windows`, `net9.0-windows` and `net48`. The packages are `Dirkster.AvalonDock` plus separate theme packages, such as `Dirkster.AvalonDock.Themes.Metro` and `Dirkster.AvalonDock.Themes.VS2013`. Its namespaces are `AvalonDock.*` rather than `Xceed.Wpf.AvalonDock.*`, so moving to it means changing the Designer's XAML namespaces and `using`s, and the in-repo Metro theme would be replaced by its own Metro theme, with WireFlow's two brush colors carried over.

Question: Dirkster.AvalonDock gives OptimaxTools.Dock a clean MS-PL starting point with current .NET builds. Should OptimaxTools.Dock be (1) a fork of the Dirkster v5.0.1 source in this solution, renamed to `OptimaxTools.Dock`, so that we can fix problems in it as this item asks, or (2) the `Dirkster.AvalonDock` NuGet package used directly, with no OptimaxTools.Dock assembly? Unless you say otherwise, I will assume (1) when I reach this item.

A: (1)

Done, under assumption (1): `OptimaxTools.Dock`, `OptimaxTools.Dock.Core` and `OptimaxTools.Dock.Themes.Metro` in the solution are the `AvalonDock`, `AvalonDock.Core` and `AvalonDock.Themes.Metro` source of Dirkster.AvalonDock 5.0.1, renamed (namespaces `OptimaxTools.Dock.*`, XAML namespace `http://schemas.optimax.com/wpf/xaml/dock`), built for .NET 10 only, and signed with WireFlow's key. The rest of the upstream repository (MVVM and serializer helpers, other themes, test apps) is left out. The Designer uses them in place of Xceed's AvalonDock, whose DLLs it no longer takes from Extended.Wpf.Toolkit (it still uses that package's property grid), and the old `Xceed.Wpf.AvalonDock.Themes.Metro` project is removed. WireFlow's changes to the Metro theme are carried over, and fixed in the process: the Ctrl+Tab window, which was white in the dark theme, now follows the theme. Checked: the solution builds without warnings, and off-screen renders of the real Designer in Dark and Light show the docked panes, the document tab and the page as before. Not yet checked by hand: docking, floating, auto-hiding and Ctrl+Tab.

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

## [DONE] XML vs JSON

Weigh the advantages and disadvantages of converting current JSON data — how the project data is stored inside WF — into XML.

Done, as an analysis; nothing is changed. Today each page, layout and part is one JSON value in a row of the `.wf` file, written by Newtonsoft.Json, with a `$type` tag on each element naming its .NET class (`"WireFlow.FlowElement, WireFlow.Types"`).

For XML:

- Element kinds become element names (`<Box>`, `<Text>`, `<Group>`), rather than .NET class names in `$type`. That would untie the files from the code: today, renaming a class or an assembly makes every saved file unreadable.
- A schema (XSD) can describe and validate the format, and XML allows comments.
- It reads like XAML, which WPF developers know.

Against XML:

- The SQLite inside a `.wf` has built-in JSON functions (`json_extract`, `json_each`, and a binary JSONB form), so a query can look inside documents, for example to find every element whose `Href` leads to a page that no longer exists, or to index links. SQLite has nothing similar for XML.
- The serializers fit worse. Newtonsoft.Json handles the model's polymorphic lists (`IList<FlowElementBase>`) and WPF value types as they are. `XmlSerializer` needs every subclass declared with `[XmlInclude]` and public setters throughout, and `DataContractSerializer` its own attributes on every class, so the model would change.
- It is about twice as large and slower to read, which matters little at WireFlow's sizes but is not nothing.
- Every existing file would have to be converted, both storages and the conversion code rewritten, and the format checked again. The 2016 OPTIMAX migration showed what a format change costs.
- Loading XAML-like markup with `XamlReader` would be tempting and unsafe, since it can create any type named in the file.
- Users never see the format: a `.wf` is a SQLite database, so readability by hand counts for little.

Recommendation: keep JSON. Its one real weakness is the .NET class names in `$type`, and that can be fixed within JSON: each element already has a `Type` (`box`, `text`, `rect`, `group`, `part`, `image`) that names its kind, so a small Newtonsoft.Json converter could choose the class from `Type` and stop writing `$type`, with the next format number (`PRAGMA user_version` 2) and a one-time upgrade of older files when they are opened. That would give the decoupling XML would bring, without its costs. If you want it, add it as a TO-DO item.

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

## [DONE] Text Component/Tool

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
- **Background:** at first a text's background was still not restored from the file; it showed transparent. The sample projects have texts saved as White (`optimax-old.wf`) and as the old tool's gray (`WireFlow.wf`); showing them would change how those pages look.

Should texts show the background saved for them?

Answer: Yes, show saved backgrounds. Make sure that it is possible to set text background to transparent, though.

Done. A text now shows the background saved for it, in the Designer and in the Browser. In the samples, `optimax-old.wf`'s texts show their white and `WireFlow.wf`'s the old tool's gray. A text saved without a background, or with one that is not a plain color, stays transparent; before, an empty one would have come out black.

To make a text's background transparent, in Properties, Background:
- clear the value: a text never has "no" background, so clearing it makes it Transparent;
- or type `Transparent`;
- or open the color dialog ("…") and set the opacity to 0 %.

A transparent text can still be selected by clicking anywhere in its frame, not only on its letters. New texts from the Text tool start transparent, as before.

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

## [DONE] BUG: Background Not Saved

User-defined page background does not survive project save/reload.

Fixed. A page had no background of its own in the file: every page was drawn in the project's background, so the one you gave the page in Properties was never saved. Now a page, or a layout, keeps the Background you give it (the canvas in Properties, with nothing selected), and it shows again after save and reload, in the Designer and in the Browser.

- A page whose background you leave alone stores none, and keeps following its layout's background, or else the project's. A background given to a layout reaches the pages that use it.
- Clearing a page's Background in Properties makes it follow its layout or the project again.
- Existing files need no conversion: their pages simply have no background of their own yet.

## [DONE] Pages Line Spacing

Pages list's lines are too far apart. Make them the same spacing as Tools palette. The same applies to Layouts, Parts, Images, and History.

Done. Pages, Layouts, Parts, Images and History now have the Tools palette's row spacing: 27 px rows in Dark and Light, as a menu item, instead of the theme's 45 px, and Classic's own compact 23 px rows. All six lists share one piece of code, so they stay alike, also after a change of theme and as new rows appear.

History is still empty: nothing fills it yet. Its rows will be as compact when something does.

## [DONE] BUG: StackPanel Crashes App

Place a StackPanel on a page. Check its `IsItemsHost` property. Click on the panel — the app crashes.

Fixed. `IsItemsHost` is a WPF panel property that hands the panel's children to a list control (an `ItemsControl`). A page element is in none, so once it was checked WPF refused every access to the panel's children ("A panel with IsItemsHost="true" is not nested in an ItemsControl"), and a click, which reads them, crashed the Designer. It means nothing for a wireframe, so Properties no longer lists it: not for stack panels, nor for groups, layers, parts and the page itself, which are panels too and crashed the same way.

## [DONE] Ctrl+S

Make it a shortcut for File|Save.

Done. Ctrl+S is File → Save, and the menu shows it. As the menu does, it saves every modified page, layout and part, and the project; a project not saved yet asks for a file name first.

## [DONE] Esc Cancel Specialized Tool

Pressing Esc should revert the tools selection to Select. 

Done. Esc chooses the Select tool again, from any other tool, wherever the keyboard is in the window: the page, the Tools list, Properties. On the page it also still does what it did: it ends a drag and clears the selection.

## [DONE] Reopen Previous

"Reopen previous project on startup" option should ensure that not only the project re-opens, but all the tabs within that project that were open get re-opened.

Done. With the option on, the Designer reopens the project with the tabs that were open when you closed it: pages, layouts and parts, in the same order, with the same tab in front.

- A tab whose page, layout or part has since been deleted or renamed is skipped.
- If none is left (or you had closed them all), the home page opens, as before.
- Tabs dragged out into a floating window are not remembered.

## [DONE] Text Alignment Custom Editor

Text Alignment Custom Editor should not be embedded into the property grid directly. It should be invoked via `...` button and shown in a popup like any other property. Only the value should be shown in the property grid directly.

Done. The TextAlignment row of a text or a box now shows only the value: where the text lies in its frame, named as the 9-point chooser names it (Top-Left, Top, Top-Right, Left, Center, Right, Bottom-Left, Bottom, Bottom-Right). Its `...` button opens the chooser in a small dialog, as Background opens the color dialog; Ctrl+Enter or a double click on the value open it too. The dialog is laid out as PDFTools shows the chooser in Add Margins and Resize Pages: the chooser in a "Text position" group, its label at the right, then OK (Enter) and Cancel (Esc). OK moves the text; Cancel leaves it.

- Justified text shows as, for example, "Top-Left (justified)", and stays justified unless you choose another position.

## [DONE] All Tools Derive from BaseDesignerObject

Make all element tools, including Text, Rectangle, Ellipse, derive from BaseDesignerObject, exposing  (forwarding) the properties that make sense, so that all can support HRef property, in preparation for the next task.

Done. Text, Rectangle and Ellipse are now BaseDesignerObjects, as Box and Image already were: each is a small control holding its WPF shape, and Properties shows the shape's properties that make sense (Fill, Stroke, StrokeThickness, RadiusX/RadiusY for a rectangle; Text, TextAlignment, TextVerticalAlignment, TextWrapping, TextPadding, LineHeight, Background, Foreground and the font for a text) and Href, under Navigation.

Found and fixed on the way:

- Ellipses were saved but never drawn again: a page with an ellipse showed a pink "unknown object" in its place after reload, and saving that page again turned it into a rectangle. Ellipses now load as ellipses.
- Duplicate copies an element through XAML, which would also write its inner shape; it no longer does, so each copy builds its own shape, and changing the copy's properties changes what it shows (checked for texts, rectangles, ellipses and boxes).
- A text's padding and a rectangle's saved alignments were lost on save; they are kept now.

## Href on All Elements

Ensure that a hyperlink target (Href) can be defined on any element (at least any that has a visual extent): Text, Rectangle, etc., not only on Image or Box.

Done. Every element with an extent now has Href, under Properties → Navigation: Text, Rectangle, Ellipse, Box and Image (from the task above), and also groups, stack panels and part references. Layers have none.

- A click follows the innermost link around what you clicked: an element's own link first, else that of the group, stack panel or part it is in. So you can link a whole group (say, a card) and still give one element inside it a different link.
- The Browser and the Designer's Navigate tool behave alike.
- Links are saved with the page; existing files need no conversion.

## [DONE] Support Undo/Redo

Redesign the app, so that unlimited Undo/Redo is supported. Undo history must be kept since the last save.

Done. Edit → Undo (Ctrl+Z) and Redo (Ctrl+Y or Ctrl+Shift+Z), without limit, for the page or layout in front, back to when it was opened or last saved.

- The history records the DOM itself: every change of every node of the page, and every element added, removed or moved. So every edit can be undone, whatever made it: drawing, moving, resizing, aligning, sizing, duplicating, deleting, grouping, ungrouping, ordering, and values set in Properties.
- A drag of the mouse or a resize is one step, however far it goes; any other edit is one step, even when it moves many elements (aligning ten elements is undone at once). Each arrow key press is a step.
- Each open tab has its own history. Saving clears it, as does opening another page in the tab. Undoing every edit since the save makes the page unmodified again.
- In a text box (in Properties, say), Ctrl+Z undoes the typing in the box, as usual.
- Tested with a script (27 checks), not by hand in the running Designer: please try Undo and Redo after the edits above.

## [DONE] BUG: TextAlignment vs TextHorizonalAlignment, TextVerticalAlignment

TextAlignment needs to be in sync with TextHorizonalAlignment and TextVerticalAlignment. When changing one of TextHorizonalAlignment, or TextVerticalAlignment, TextAlignment's highlight remains where it was. In custom editor shown only on demand, it should not be a problem, but the value should be in sync with the other two.

Done. The TextAlignment row follows the other two as they change: set TextVerticalAlignment to Bottom, and it reads Bottom-Left at once; the dialog always opens at the current position.

- On a box, changing TextHorizontalAlignment now moves the lines with the text (TextAlignment follows: Left, Center or Right), so the two no longer disagree. Stretch leaves TextAlignment as it is.
- A text (not a box) now lists its TextVerticalAlignment under Appearance too, as a box does; before, only the chooser could set it.
- Saved boxes keep their own TextAlignment when loaded, so existing pages look as they did.

BUG: TextHorizontalAlignment is no longer even shown.

Fixed. A box listed it (under Text, at the end), but a text did not. Both now do, and on both, setting it moves the lines with it (TextAlignment follows: Left, Center or Right), and the TextAlignment row and its chooser follow it; the chooser sets it too.

## [DONE] BUG: Properties of Multiple Objects

When multiple objects are selected, the Properties grid should show the properties that all of the selected objects have in common, not the last selected object's properties. Changing a value of such property should apply to all the selected objects.

Done. With several objects selected, Properties shows the rows that all of them have (a rectangle and a text share Href, position, size and Transparency; two texts share all of a text's rows).

- A value shows where all the objects have the same one, and is empty where they differ.
- A value set is set on all of them, and one Undo takes it back from all.
- Name is left out: setting it would give all of them the same name.
- Rows are matched by the name they are shown under, so a text's Background (its fill) is not shared with a rectangle's Fill.
- Tested with a script, not by hand in the running Designer.

## [DONE] Color Selection Dialog

Color Selection Dialog needs a number of predefined swatches near the bottom: Black, White, Light Gray (#dddddd), and Gray (#999999). Additionally, it needs to remember and show swatches for the last 10 colors selected. Clicking such a swatch would set the dialog's color to what the swatch represents. 

![pasted-5](<TO-DO.images/pasted-5.png>)

Remove the above-highlighted control in the middle.

Done. Under the color wheel and sliders: a Standard row (black, white, light gray #dddddd, gray #999999) and a Recent row with the last 10 colors chosen with OK, the latest first, each shown once. A click on a swatch makes it the dialog's color; its tooltip gives its #AARRGGBB; translucent colors show over a checkerboard. The recent colors are kept in the Designer's settings, from run to run. The two-color control in the middle is gone.

## [DONE] Convert to a DOM (Both)

This applies to both the Designer and Browser. 

Currently, the WPF element tree itself is the Designer’s working model. The Flow* data model only exists while a page is being loaded or saved. Redesign the flow so that the DOM becomes the source of truth and the ultimate authority about the model, independent of its presentation. Treat WPF as a presentation layer only, one of possibly many. The saved data is another "presentation". Make sure that WPF-specific details do not spill into other layers or services.

Plan, in phases; each one is committed on its own, with the Designer and the Browser working after it:

1. A WPF-free DOM. The Flow* classes (`WireFlow.Types`) become the DOM: they lose every WPF type (alignments, text wrapping, font stretch, image stretch, stack direction become WireFlow's own values with the same names, so `.wf` files stay exactly as they are), and the project no longer uses WPF.
2. Observable DOM. Nodes announce changes to their properties and children, and know their parent.
3. WPF as a view of the DOM. The Designer keeps each open page, layout and part as its DOM; the WPF elements are drawn from it and follow its changes; saving writes the DOM, not the WPF tree.
4. Edits go through the DOM. Properties shows the selected element's node; drawing, moving, resizing, deleting, duplicating, grouping and ordering change nodes, and the view follows. This is also what Undo/Redo (the next task) will record.
5. The Browser draws the DOM through the same view, read-only.

Done, all five phases; the questions below were answered as the work had assumed. Phases 3 to 5 were done together rather than one by one: with saving writing the DOM, an edit still made on the WPF elements would have been lost, so the view following the DOM and the Designer editing it had to arrive at once.

- The DOM is the model. The Designer keeps each open page or layout as its DOM, and saves the DOM; the Browser draws the DOM it reads. The WPF elements are a view: each is tied to its node and follows it, in place, so the selection stays on it; adding, removing or reordering a layer's or group's nodes adds, removes or reorders their elements.
- Every edit changes nodes: drawing, moving (mouse and arrow keys), resizing, aligning, distributing, stacking, sizing, duplicating, deleting, grouping, ungrouping and ordering. Undo/Redo can record them at the DOM.
- Properties shows the selected element's own properties for its kind (a rectangle no longer lists fonts, margins or cursors), under the names it had (TextPadding, BorderColor, TextVerticalAlignment, ...), and the page's size and background when nothing is selected. Colors keep the swatch and the color dialog; the text position keeps its chooser.
- Saving no longer loses detail: color names stay names, a rectangle without a stroke keeps none, a group's size is not fixed. All 8 pages of the sample project, drawn and saved unchanged, write the same JSON.
- Tested with scripts (view and Properties: 26 checks; the Designer's operations: 18 checks), not by hand in the running Designer: please try drawing, moving, resizing, grouping and Properties.

Q1: I am making the existing Flow* classes the DOM (observable and WPF-free), rather than adding a separate set of DOM classes beside them, and keeping the `.wf` format unchanged. OK?

A1: Yes.

Q2: After phase 4, Properties shows an element's own properties (Fill, Stroke, Text, Href, position and size, font, alignment and so on), no longer every WPF property of the control that draws it (Margin, Cursor, IsHitTestVisible, Focusable and others, which are not saved anyway). OK?

A2: Yes. We may have to adjust and show specific additional properties later on.

Q3: Colors stay text in the DOM (`#AARRGGBB` or a color name), as the files keep them; the color dialog keeps working on them.

A3: Yes.

## [DONE] BUG: Image Crashes App

Placing an image object on a page crashes the app.

Fixed. The image dialog still asked for a style of the old look, which the themes no longer have, so it failed as it opened, and the Designer with it. It now opens, lists the project's images, and the image chosen is placed.

- A placed image now keeps its Uniform stretch: it was saved as None and drawn at its natural size.
- A project without images opens the dialog with an empty list rather than failing.
- Checked with a script that builds the dialog and places an image; not by hand in the running Designer.

## [DONE] Stack Panel Content in Outline

A Rectangle inside a StackPanel does not show up in the Outline. Any objects placed inside a StackPanel should be listed and be selectable in the Outline.

Done. The Outline lists the elements of stack panels, in their order in the panel, and of groups (topmost first, as a layer's), and a click there selects any of them.

## [DONE] Stack Panel HitTest

An object inside StackPanel cannot be selected by clicking on it. The entire panel is being selected on click, instead of individual objects.

Expected: When an object inside the StackPanel is clicked on, that object becomes selected. Pressing Esc should unselect object but select it's parent/owner container. Eventually, the Page  itself becomes selected.

Done. A click on an object in a stack panel selects that object; Esc selects the panel (or group) holding it, and Esc on a top-level object leaves nothing selected, which shows the page in Properties. So that a panel full of objects can still be moved with the mouse: once the panel is selected, a drag that starts on one of its objects moves the panel, and a click without a drag selects the object. Objects in a group are still selected as their group on the canvas (they are meant to move as one); the Outline selects them one by one. Delete, duplicate and the order commands work on a selected object in a panel; dragging it or the arrow keys do not move it, since the panel places it. Checked by a script that drives the Designer's mouse handlers; not by hand.

## [DONE] Avoid Rebuilding Entire Outline

Avoid rebuilding the entire outline on every change. Rebuild/update only the elements of the outline that are affected by the change. Make outline virtualizing, so that when the tree gets long and complicated, it renders fast.

Done. After a change the Outline is brought up to date in place: the entries of the elements that stay are kept as they are (expanded or collapsed, selected), and only the entries of elements added, removed or moved change. A rename (in Properties, or by undo) shows in its entry at once, without any update of the tree. The tree is virtualizing: only the entries in view are drawn. Checked by a script against a sample project; not by hand.

## [DONE] TextHorizontalAlignment

Is missing from Text and Box elements.

Done with the bug above: texts and boxes both list TextHorizontalAlignment under Text, in sync with TextAlignment.

## [DONE] NodeProperties

![pasted-3](<TO-DO.images/pasted-3.png>)

The Properties grid should identify the type of the currently selected node, instead of saying "NodeProperties" for everything.,

Done. The header names what is selected: Rectangle, Ellipse, Text, Box, Image, Group, Stack Panel, Part, Page or Layout, followed by its name; with several selected, "3 objects". Checked by a script; not by hand.

## [DONE] Property Grid and Arrow Keys

Up and Down arrows should navigate up and down in the property grid, like in a spreadsheet.

Done. Up and Down move to the value of the row above or below, across categories, scrolling as needed; a value typed is set as you leave it, as with Tab. An open drop-down list keeps Up and Down to choose in it, and so does a text of several lines while there are lines to move to. (On the way, the ID row from the item above did not show; it does now.) Checked by a script that presses the keys in a grid; not by hand.

## [DONE] Remember Window Position and Panes

Make sure that the window position, layout (the open panes), and the splitter positions are remembered from run to run.

Done. The Designer saves, as it closes, the window's place and size (and whether it is maximized), and the docking layout: which panes are docked where, floating, auto-hidden or closed, and the sizes the splitters give them. The next run opens the window where it was, before it shows, and puts the panes back. A window last placed on a screen no longer attached opens where Windows puts it. The restore of the old window position never worked (its test was the wrong way round); it does now. The History pane had the same pane ID as Layouts, which a saved layout cannot tell apart; it has its own now. A script saved a layout and read it back (sizes and a closed pane survive); the real window was not tried by hand.

BUG: Splitter positions are note being saved.

The Designer you tried was the build published before this change (19:45): the settings it wrote at 21:26 hold no layout. The build published at 21:48 saves the panes and the sizes the splitters give them (a drag of a splitter sets the size the layout keeps, and a script showed sizes surviving a save and a restore). Please try that one: drag a splitter, close the Designer, start it again.

## [DONE] BUG: Float a Pane Crashes App

Setting a pane to Float, crashes the application.

Fixed. The crash came as a floating pane was dragged, when the Designer shows the drop targets: WPF's Fluent theme gives every window shown without a style of its own Fluent's plain window look, which replaced the look of the docking windows, so the drop-target window lacked its parts. The floating windows were affected too. The docking windows now name their own look before they show. Checked by a script that reproduced the crash with the Designer's themes and then, fixed, showed the drop targets and a floated pane in their own look, light and dark; not by hand.

## [DONE] Add Cut/Copy/Paste to Context Menu

Add Cut/Copy/Paste to the Context Menu of the Designer.

Done. The page's right-click menu now starts with Cut, Copy and Paste (with their keys shown), then Duplicate and Delete. They are the Edit menu's own commands, so they are on and off just as those are: Cut and Copy need a selection, Paste needs something copied, and all three are off in Navigate mode. Paste puts the elements where Edit > Paste does (where they were copied from, moved 10 px while they would cover a copy), not at the point clicked. Checked by a script on a copy of Data/WireFlow.wf (11 checks); not by hand.

## [DONE] Navigate Mode in Designer

Make it possible to switch the Designer (toggle with a toolbar button) between Design and Navigate mode. Navigate mode should behave like the Browser. 

Done, with two questions. The toolbar's compass button (also View > Navigate Mode) switches between Design and Navigate mode. In Navigate mode every open page behaves as in the Browser: a click on a link shows its page (in the same tab, or the tab that already shows it), nothing can be selected or changed (Tools and Properties are off; Undo, Paste and the like are disabled), and Back and Forward (toolbar, View menu, Alt+Left and Alt+Right) go through the pages visited. Switching back gives every page the Select tool. Checked by a script that drives the real window; not clicked through by hand.

Q1: The Navigate tool is still in the Tools list (it follows links in Design mode). Remove it now that there is Navigate mode? I left it.

A1: Actually, remove the Toolbar button for now (but not the code behind it, in case we want to surface it again). The way the tool works is sufficient. 

Q2: Do you want a key to switch modes (F6 is free, for example)? I gave it none.

A2: If F5 is free, then make it F5. Otherwise F9. The key should toggle the mode —select/deselect the Navigate tool.

Done as answered. The toolbar's Navigate Mode button is gone (kept in the XAML as a comment, to be put back). F5 is Reload from File, so F9 switches Navigate mode, as does View > Navigate Mode. The Navigate tool is now the mode itself: choosing it in the Tools list turns Navigate mode on (for every open page), and choosing any other tool turns it off with that tool; F9 off gives the Select tool. The Tools list therefore stays usable in Navigate mode; Properties is still off. Checked by a script on the real window; not by hand.

## [DONE] Property Grid Highlight

The highlight of the currently selected row in Properties grid is almost the same color as the grid's headings. Make it a really light yellow (#FFFFF6E1) and provide a way of defining its color in Tools > Options, as well as a toggle whether to display it at all.

Done, as you said in chat: the whole row, and no yellow. The row being edited in Properties is highlighted across its whole width in the Windows accent color, partly transparent, so it shows in Light, Dark and Classic and is clearly not the gray of the headings. Tools > Options has a new Properties page: "Highlight the row being edited" (on by default) and "Highlight color" (the accent, or blue, green, orange, purple, gray). Checked by a script that rendered it in all three themes; not by hand.

## [DONE] Navigate Mode Indicator

Add a colorful indicator to the workspace that would show the navigation mode is on. The color and text color of which is settable via Tools > Options. See rough illustration below (shown in red):

![pasted-10](<TO-DO.images/pasted-10.png>)

Done. While Navigate mode is on, an orange badge reading "Navigate Mode" (white text) hangs from the top edge of each page, at its center, under the tabs, as in your drawing; clicks go through it to the page. Tools > Options has a new Navigate Mode page: "Indicator color" (orange, red, blue, green, purple, teal, yellow, charcoal) and "Indicator text color" (white, black); a color typed into the settings file is offered as Custom. Checked by a script that hosted the window and rendered it; not by hand.

## [DONE] Renaming Pages

Currently, a Page `Name` is more than a field in the page’s JSON. It’s also:

- the key of the page’s row in the .wf file (Documents, by Kind and Name);
- its entry in the project’s page order (PageOrder);

- its item in the Pages list and its tab’s title.

Renaming a page has to change all of these together, and only Engine.RenamePage does that: it renames the row in the file at once and keeps the page’s place in the order. The Pages list’s Rename command (MainDesignerWindow.xaml.cs:2625) uses it. The row was marked read-only in the DOM rework (ebb5494) to prevent changing it. And that's the problem. 

#### Solution:

An internal (hidden), program-assigned, page GUID (no hyphens, no brackets) should be the key in all cases involving saving and ordering. It should not be displayed to the user but used internally as the Unique Page Identifier (UPI) and a key to the database.

The Page Name should be a user-facing feature that allows the user to meaningfully name the page. It also can serve as a target of Href.

Page ID is a short identifier, with hyphens permitted, but not spaces, to also serve as targets of HRefs. Moreover, the page ID should be changeable from the Properties, just like Name and - when changed - should adjust all the Hrefs that contain it. 

All Hrefs should operate on the basis of IDs or names (check ID first). When ID is entered into an Href, it stays as ID; when a name is, it stays as a name. 

Currently rename page works only from the Pages list. Make it work from Properties.

Done. Every page now has a hidden key, a GUID of 32 hex digits, which the .wf file stores pages under, and which the page order, the tabs, the Pages list and Navigate mode's history use. It never changes and is never shown. The page's Name is just its label: it can be set in Properties (no longer read-only) as well as by Rename in the Pages list. A name another page already has, or an empty one, is refused. The page keeps its place and its open tabs. Links resolve an ID first (of a page, or of an element or layer on one), then a page's name. A link keeps what it names: an ID stays an ID, a name stays a name. Changing an ID in Properties (of a page or any element), or renaming a page, changes the links that named the old value. That covers open pages as an edit you can undo, and pages that are not open directly in the file. Checked by a script on a copy of Data/WireFlow.wf (17 checks); not by hand.

Please note: the file format is now 3. A format 2 file is upgraded in place, in one transaction, the first time the new Designer opens it, and an older build can no longer open it afterwards. Undoing a rename made in Properties gives the page its old name back in the page itself, but the Pages list keeps the new name until the page is saved. Layouts and parts are still known by their names; that is the next item.

## [DONE] Renaming Layouts

Currently, renaming a layout isn’t implemented at all. SqliteRepository.RenameLayout throws NotImplementedException (SqliteRepository.cs:296), because pages refer to their layout by name (the page’s Master). A rename would also have to rewrite the Master of every page, and every layout, that uses it. Parts are the same: elements refer to a part by name, and RenamePart throws too.

Make analogous changes as to the Pages, so that renaming layouts is easy. 

NOTE: Any page can become a Layout. Eventually, a Layout could be instantiated as a Page. Normal use for Layouts is to provide the background for a page (or another layout).

Done, as for pages. Layouts now have hidden keys too. A layout can be renamed in Properties (Name); a name another layout has is refused. A page's or layout's Layout (Master) can name a layout by its ID or by its name: an ID is tried first, then a name. Renaming a layout makes the pages and layouts that named its old name name the new one. Changing a layout's ID does the same for those naming its ID. New layouts get an ID made of their name, as new pages do. Two things changed along the way. A layout is now read with its unsaved edits, as a page is, so a new layout opens at once and its edits show before Save Project. And layouts set on each other in a circle no longer hang the drawing: the chain stops at the layout it comes back to. The New Page and New Layout dialogs still list layouts by name. Checked by a script on a copy of Data/WireFlow.wf (15 checks); not by hand. Choosing the Layout from a list in Properties is the next item.

## [DONE] Layouts in Pages List

Layouts should not appear in the list of Pages, only in Layouts.

Fixed. The Pages list never listed layouts. Save Project, and Yes when the Designer asks as it closes, saved every open tab as a page. So a layout edited and saved became a page of the same name, and that page showed in the Pages list. Each tab is now saved as what it is: a page as a page, a layout as a layout, a part as a part. Checked by a script on a copy of Data/WireFlow.wf (10 checks); not by hand.

The bug left its mark in Data/WireFlow.wf. Its page "NormalLayout" holds the blue header (a 900 x 52 rectangle) that was drawn on the layout, while the layout "NormalLayout" itself is still empty. The file is yours, so I left it as it is.

Should I repair Data/WireFlow.wf by moving the page's content into the layout and deleting the page? Or will you do it yourself (cut the rectangle from the page, paste it into the layout, then delete the page)?

A: I'll do it myself.

## [DONE] Editing Layouts

Layouts, just like pages, must remain editable. Anything you can do to a page, you can do to a Layout. Changing a Layout must change any open Pages that depend on it.

NOTE: Prevent cycles in Layout dependencies. A Layout pipeline is a chain, never a cycle.

Done. A layout opens in its own tab and is edited there as a page is: the same tools, commands, Outline, Properties, Undo and Cut/Paste. I found nothing a page can do that a layout tab cannot. The pages and layouts open now show a layout as it is being edited, before it is saved. An element drawn, moved, recolored or deleted on the layout, its background, and an undo or redo all show at once on every open page or layout set on it, nearer or farther. Closing the layout's tab without saving takes those edits back off the pages.

Cycles are refused wherever one could be made. Setting a layout was covered already. Now renaming a layout, or giving it an ID, to a name or ID that a layout under it names as its layout is refused too, and so is creating a new layout of such a name. Drawing still stops at a layout already in the chain, in case a file has a cycle. Checked by a script on a copy of Data/WireFlow.wf (11 checks); not by hand.

I noticed one related thing: closing a tab with unsaved changes discards them without asking. That holds for pages, layouts and parts alike, and only Save Project or closing the Designer saves them. It matters more now, since closing a layout's tab also takes its edits off the open pages.

Should closing a tab with unsaved changes ask Save / Don't Save / Cancel, as AGENTS.md describes for dirty documents? (Save would keep the edits as the document's draft, as Save Project does before it commits.)

A: yes.

Done as answered. Closing a page, layout or part tab with unsaved changes now asks "Save changes to the page "Page 2"?" with Yes, No and Cancel. Yes saves it as what it is (as its draft, which Save Project then commits, as before), No drops the changes, and Cancel keeps the tab open. A tab without changes closes without asking. I extended this to closing the project: New Project, Open Project (and the recent files), Reload from File and Close Project dropped every unsaved change silently. They now ask "Save changes to the project?" once, as closing the Designer does, and Cancel stops the command; the tabs then close without asking one by one. Checked by a script on a copy of Data/WireFlow.wf (12 checks); not by hand.

Publishing: the last three items are built (Release, no errors) but not published. publish-locally.bat could not replace c:\Programs\Optimax\WireFlow, because WireFlow Designer is running from there. I did not close it, since it may be yours. I will publish again after the next item; if it is still running then, run publish-locally.bat once you close it.

A mishap on my side: three runs of my test scripts hung while closing their hidden window, on the Designer's "Save changes?" prompt, and were killed before they could put your settings file back. %APPDATA%\WireFlow\WireFlow.Designer.config kept your theme, colors and pane layout. It lost your window place and size (they are the test window's now: Windows will place it again), and Reopen the previous project at startup is off. I cleared the previous project and tabs, which pointed at a temporary test file. Please switch that option back on in Tools > Options if you had it on. The scripts now keep a backup of the settings and cannot hang on that prompt.

## [DONE] Highlight Link in Navigate

Make a toggle switch (and a Toolbar button) to highlight all links on a page to make them visible. Highlight them with a semi-transparent overlay, the color and opacity of which can be adjusted with Tools > Options.

Done. View > Highlight Links (Ctrl+L) and a toggle on the toolbar, after Back and Forward, lay a semi-transparent overlay over every link on the pages open. That covers the page's own elements, those inside groups and stack panels, part references, and the elements of its layouts. The overlay follows the page: a link made or taken away, an undo, another page shown, or a layout redrawn lays the overlays again. Clicks go through them. Tools > Options > Navigate Mode has Link highlight color (Blue by default, also Yellow, Green, Orange, Red, Purple, Teal, Gray) and Link highlight opacity (20% to 60%, 35% by default).

Assumptions I made: it works in Design mode too, not only in Navigate mode, since it is just as useful when checking links while editing. Its on or off state is kept in the settings. Its key is Ctrl+L, which nothing else used. Checked by a script on a copy of Data/WireFlow.wf (10 checks); not by hand.

## [DONE] Zoom Controls and Fit Page

Add zoom controls to the toolbar and commands to the menu, similar to those in MarkdownTools. Also, add a "Fit Page" button and command to zoom-to-fit.

Done, as in MaxWrite. View has Zoom In (Ctrl+=), Zoom Out (Ctrl+-) and Actual Size (Ctrl+Shift+0), in steps of 10%, and Fit Page (Ctrl+0), as in PDFTools. The toolbar ends with the same group as MaxWrite's: Zoom Out, the zoom of the page in front ("100%"; a click goes back to Actual Size), Zoom In, and then Fit Page, with PDFTools' magnifier and page icons. Zooming keeps the point at the center of the view in place; the mouse wheel still zooms about the point under the mouse, in the same steps. Fit Page shows the whole page as large as it fits, with a little room around it, and centers it.

Assumptions I made: each tab has its own zoom, and a tab opens at 100% (the zoom is not kept in the settings). The zoom goes from 10% (low enough for Fit Page to show a long page whole) to 500%; MaxWrite stops at 25%. Fit Page zooms once; it does not keep fitting as the window is resized. Ctrl+0 for Fit Page is the usual key for it in PDF viewers; nothing in the Designer used it. Checked by a script on a copy of Data/WireFlow.wf (19 checks); not by hand.


## [DONE] BUG: Adding Images to Project

![pasted-8](<TO-DO.images/pasted-8.png>)

There is no way to add an image to the project. Also, OK and Cancel buttons are clipped rendering their text almost invisible.

Fixed. The dialog's + and − did nothing: they were copied from the Images pane without the commands behind them. Now + imports image files into the project (several at once) and selects the first, and − deletes the selected image after asking; the Images pane shows the change. OK and Cancel are full size (Enter is OK, Esc Cancel). Checked by a script that opened the dialog with the Designer's theme (both buttons enabled as they should be, OK and Cancel whole); the file dialog and the delete were not tried by hand.

## [DONE] BUG: Color Selection Dialog Controls

Color Selection Dialog (Fill, Background, Stroke, etc.) overlaps color component textboxes. Also, it does not provide enough room for the "Web color" box, especially, when the focus is on it and the clear-X appears.

![pasted-4](<TO-DO.images/pasted-4.png>){width="200" style="max-width: 80%"}

Fixed, with the item below. The H, S, V and A values (and R, G, B, L) show in full: the theme's text boxes were wider than the room the sliders give them. The Web color box is wider, so #AARRGGBB shows whole with the clear-X. Checked by drawing the dialog to an image with the light theme; not by hand.

BUG STILL THERE: 

![pasted-7](<TO-DO.images/pasted-7.png>)

The text boxes are not high-enough.

Fixed. The color picker fixes those boxes at 18 pixels high, and the theme's text box keeps 4 of them for its border and inner margin, which left 14 for text 16 high. The boxes now take the height their text needs (20), on every tab. Checked by a script that measured them and drew the dialog in the light theme; not by hand.

## [DONE] Stack Panel Behavior

![pasted](<TO-DO.images/pasted.png>)

Place a rectangle inside a vertical StackPanel. --> The rectangle does not span the panel horizontally as it should but seems clipped on the right instead.

Fixed by the conversion to a DOM: a rectangle drawn inside a vertical stack panel now spans it (checked by a script: an 800 px wide panel, a rectangle drawn 430 px wide inside it shows 800 px wide at its left edge, and stays so after its node changes). The rectangle keeps its own width in the file, so it comes back if it is taken out of the panel.

TODO: This is not working. The last rectangle in WireFlow > Page 2 still does not span the width of the stack panel. Apparently, only the last object in StackPanel does not get properly stretched.

Fixed. The rectangles did span the panel, but each still showed only as much of itself as its own width: when the panel widened a child, the child was first laid out in a slot of its old width, and WPF kept it clipped to that slot. On Page 2 that hid the right part of the first and the last rectangle (the second, 745 of 796, showed nearly whole). The panel now places each child at its new size itself. Checked by scripts on a copy of WireFlow.wf, Page 2, drawn as the Browser and as the Designer draw it, also after editing, drawing another rectangle into the panel, undo and redo; not by hand.

## [DONE] New Layout Dialog Look

The New Layout dialog does not give its controls sufficient height, making the text clipped.

![pasted-9](<TO-DO.images/pasted-9.png>)

Fixed. The dialog had a fixed height and rows and buttons sized for the old theme; it now takes the height its fields need, with OK and Cancel at the right, full size. New Layout and New Page are the same dialog, and New Part, which had the same fault, is fixed too. Checked by a script that opened them with the theme (every field and button whole) and drew New Layout; not by hand.

## [DONE] ID Property

In addition to Name, every element should define a common property ID. The ID should be the target/content of an Href link.

Done, under the assumptions below. Every element, and every page and layout, has an ID in Properties (under Common, after Name), empty until you set one; it is saved with the project. A link (Href) can name a page, as before, or an ID: the ID of a page leads to that page, and the ID of an element leads to the page that holds it. This works in the Browser and with the Designer's Navigate tool. Names and IDs compare ignoring case, as page names do. With several objects selected, Properties does not offer ID (each keeps its own), as it does not offer Name. Checked by a script against a copy of a sample project; not by hand.

Q1: Should a new element get an ID by itself (say `rect-12`, unique in the project), or stay without one until you give it one, as now?

A1: Give it an automatic one.

Q2: Should the Designer refuse an ID that another element or page already has? Now nothing checks, and a link goes to the first page found that has it.

A2: Refuse, if it is not too expensive to check. Otherwise the first found link.

Q3: When a link names an element's ID, the Browser opens the page holding it. Should it also show which element (scroll it into view, mark it briefly)?

A3: No. Just open the page.

Q4: Should links keep naming pages by name, or should they name IDs only? Both work now; a page's name is tried first.

A4: IDs only. Change the Hrefs of existing projects accordingly.

Done, as answered. A new element gets an ID by itself, its kind and a number (rect-1, ellipse-2, ...), unique in the project; so does a copy, and a new page gets one made of its name ("Contact Us" gives contact-us). Properties refuses an ID that another element or page already has, and says so. A link leads to the page with that ID, or to the page holding the element with that ID; a page's name alone no longer works. The projects in Data were converted: each page got an ID made of its name (Page 2 is page-2), and each link that named a page by its name now names that page's ID; WireFlow.wf and FirstTest.wf, yours, are converted but not committed (copies from before are kept). New layouts and parts get no ID by themselves, and layers none, as links lead to pages. Checked by a script on a converted copy of WireFlow.wf (14 checks); not by hand. If the Designer had WireFlow.wf open while it was converted, a page it saves afterwards writes back its old links: close it without saving, or convert it again with Documents\upgrade-links-to-ids.cs.

## [DONE] Outline Caption and Menu

![pasted-6](<TO-DO.images/pasted-6.png>)

Add an Add Layer and Delete small buttons in the Caption of the Outline, to make it possible to insert new Layers and to delete the selected objects. Add corresponding commands to the Outline menu.

Done. The Outline's caption has two small buttons, where the picture shows them: Add Layer (a new layer, "Layer2" and so on, above the current one, which it becomes) and Delete (the selected objects; with none selected, the layer chosen in the Outline, with what is on it; a page keeps one layer at least, and the layers of its layout cannot be deleted from the page). Both can be undone. A right-click in the Outline offers the same two commands. The layer operations are checked by a script; the buttons and the menu were not tried by hand.

Q1: By "the Outline menu" I took the menu a right-click in the Outline opens. Did you mean the pane's ▼ menu instead (Float, Dock, Auto Hide...)?

A1: No, let's keep it just to the context menu for now..

Kept as it is: the two commands are in the Outline's right-click menu only.

## [DONE] View Menu Toggles

For each pane in the application (Outline, History, Tools, Pages, Layouts, etc.), make it possible to toggle its visibility via View menu. 

Done. The View menu lists every pane under Theme (Tools, Pages, Layouts, Parts, Images, History, Outline, Properties), checked while it shows; choosing one hides or shows it, and a pane shown again goes back where it was. Each pane's caption X now hides it too (it was disabled), and the View menu brings it back. A hidden pane stays hidden next time, as part of the remembered layout. Checked by a script; not by hand.

## [DONE] Pages: An Ordered List

Pages should be an ordered list, with move up and move down buttons in the caption bar, as well as drag-and-drop supported. The first page in the list is the default entry point for the model.

Done. The Pages list keeps the order you give it, saved in the project: the Pages caption has Move Up and Move Down buttons for the selected page, and a page can be dragged to its place (a line shows where it goes). The first page is where the project starts, in the Designer and the Browser. A project never ordered starts with its home page first and the others by name, so it starts where it did. A new page comes last, and a renamed page keeps its place. The order is saved at once, as adding, deleting and renaming pages are, not at Save Project. Checked by a script; the buttons and dragging not by hand.

## [DONE] Font Family in Properties

Font Family should have a custom editor that brings up a font chooser and sets multiple font-related properties, not just the FontFamily.

Done. FontFamily in Properties keeps its text box and has a "…" button (Ctrl+Enter or a double click on the value open it too) that opens the font dialog of MaxPad and MaxWrite (OptimaxTools.FontSelection, now in WireFlow's LocalPackages), on the element's font. OK sets the family, the size, and the face: its weight, stretch and style. Texts and boxes did not have italic; they have a FontStyle now (Normal, Oblique, Italic), shown under Text and saved with the page, so an italic face chosen is kept. With several elements selected, each gets the font. Checked by a script; not by hand.

## [DONE] Dirty Indicator

Mark the tab of an unsaved Page that was modified the same way MaxPad marks its dirty document tab. Also, indicate the dirty document by an asterisk in the window caption. Also, create a status bar section where "Modified" can be indicated the way MaxPad does it.

Done. A page, layout or part with changes not saved has a small orange dot after its name in its tab, as MaxPad's tabs have (the same size and color, read off MaxServices.Core's tab). The window caption has an asterisk after the project's name, "[Project]* WireFlow Designer - folder", while any page open has changes not saved: the caption names the project, not a page, so the asterisk says that something in it needs saving. The status bar's first section says Modified or Saved of the page in front, as MaxPad's does. Checked by a script (the dot); the caption and the status bar not by hand.

## [DONE] Outline Glyphs

The leading glyphs in the Outline look like they are disabled, because of the gray elements. Make the glyphs more closely resemble the Tool palette glyphs in style.

Done. The Outline's entries now have the Tools palette's line glyphs, drawn as the palette draws them, in the row's own text color, so they follow the theme and no longer look disabled: a rectangle, ellipse, text, box, image and stack panel have their tools' glyphs, and layers, layouts, groups and parts new ones in the same style. Checked by a script; not looked at by hand.

## [DONE] Outline & Tools Background

Should be the same as that of Pages in any theme, including Light and System.

Done. The Tools list and the Outline now have the background Pages has, in every theme: they took the theme's own list background before, which differs from it in Light and System. Checked by a script in Light and Dark; not looked at by hand.

## [DONE] Settings Toggle to Select Pointer Tool

Add a toggle setting to auto-reset the tool to plain "Select" after each object is placed (without having to press Esc).

Done. Tools > Options has a Drawing page with "Return to the Select tool after drawing an object", off by default (the Designer behaves as before until it is turned on). When on, the tool goes back to Select as soon as an object is drawn, as Esc does. Checked by a script; not by hand.

## [DONE] Copy/Paste, Cut/Paste

Make these sequences work. Right now they don't seem to.

Done. The Edit menu's Cut, Copy and Paste were placeholders with no command; they now work, with Ctrl+X, Ctrl+C and Ctrl+V. Paste puts the elements where they were copied from (on another page, the same place), or 10 px down and right while they would lie on their originals, and selects them; each gets a new ID, and Undo takes a paste back. A text box being typed in keeps Ctrl+X/C/V for its own text. Checked by a script; not by hand.

## [DONE] Toolbar Buttons

Add Toolbar buttons in the same style as in PDFTools PDFWorkshop for all commands that it makes sense.

Done. A toolbar under the menu, in PDF Workshop's style (flat buttons with a frame under the mouse, line icons, the theme's colors, lines above and below): New, Open (PDF Workshop's folder and its name), Save | Undo, Redo | Cut, Copy, Paste, Delete | Group, Ungroup, Bring to Front, Send to Back | Align Left, Center Horizontally, Align Right, Align Top, Center Vertically, Align Bottom | Distribute Horizontally and Vertically, Make Same Width and Height. Each runs its menu command and is dimmed when that cannot run; tooltips name the keys. The hidden, unused old toolbar is gone. Checked by rendering it in Light, Dark and Classic; not clicked through by hand.

