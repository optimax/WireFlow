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

