---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Convert to WPF Fluent Themes

Convert the application chrome to WPF Fluent themes, with the selection of Light, Dark, System, and None/Classic (like PDFTools and other Optimax apps). Make the theme selection available under View|Theme menu (place View after Edit).

Question 1: PDFTools' themes come from your `MaxServices.Core` library (`AppTheme`, `ThemeChoice`, `Palette`, `FluentStyles.xaml`; Optimax.Shared.Lib). To make WireFlow behave like PDFTools, I am copying `MaxServices.Core.1.1.0.nupkg` from PDFTools' `LocalPackages` into a new `LocalPackages\` folder here, with a `nuget.config`, as PDFTools does, rather than writing a second theme implementation. The package has no dependencies of its own. Unless you say otherwise, I proceed this way.

A: Yes.

Question 2: the Designer's current dark look becomes its Dark theme, and the default choice is System, as in PDFTools, so on a light Windows the Designer will now start light. The page and the parts drawn on it keep their own colors; the gray area around the page follows the theme. The choice is kept in the Designer's existing settings (`ConfigSettings`). Unless you say otherwise, I proceed this way.

A: Yes.

Question 3: does the Browser get the same themes? It uses the Infragistics MetroDark theme today, and this item says "the application chrome". I am converting the Designer first and leaving the Browser as it is until you answer.

A: Yes, the same as Designer.

Done: the View menu of the Designer (after Edit) and of the Browser has Theme → System, Light, Dark, Classic, as in PDFTools, and each program remembers its choice (the Browser in a new `%APPDATA%\WireFlow\WireFlow.Browser.config`). Menus, toolbars, panes, pane titles, the Outline, the Properties pane, the docking chrome and the area around the page follow the theme; the page keeps its own colors. The Browser no longer uses the Infragistics MetroDark theme, and its package is removed. Checked in off-screen renders of both real windows in Dark, Light and Classic. Not yet checked by hand, and AvalonDock's Ctrl+Tab window stays white.

## Convert Data to SQLite

Convert the WireFlow project files and subordinate folders into SQLite databases, with `.wf` extension. Figure out appropriate schema to support the functionality. Convert both Designer and Browser to use `.wf` files.

Plan: a new `WireFlow.Repository.Sqlite` project implements the same `IFlowRepository` as the folder storage, so the rest of both programs is unchanged. A `.wf` file holds:

- `Project`: the project's settings (size, background, home page) as the same JSON as the `.project` file, and its revision number.
- `Documents`: one row per page, layout and part (kind, name), with its saved JSON and, while it has unsaved edits, their JSON (what the `.new` files hold today).
- `Archive`: earlier revisions of the project, pages, layouts and parts (kind, name, revision, JSON), as the `*.archive` folders hold them.
- `Images`: one row per image (name, bytes).

Each document stays one JSON value, as written today, rather than a table per element type: the element model is open-ended (`$type` tags), and the programs read and write whole pages. The file carries a format number (`PRAGMA user_version`), so that a later format can be recognized.

**Question 1: the SQLite library is `Microsoft.Data.Sqlite` (MIT, Microsoft), with the `SQLitePCLRaw` native bundle it depends on (Apache 2.0; SQLite itself is in the public domain). I am using 9.0.2, the version already in the NuGet cache here, and adding their license texts to `THIRD-PARTY-NOTICES.md`. Unless you say otherwise, I proceed this way.**

**Question 2: opening a `.project` in the Designer converts it once into a `.wf` file of the same name beside it (`WireFlow.project` → `WireFlow.wf`) and opens that; the folder is left as it is. New projects are `.wf` files. Unless you say otherwise, I proceed this way.**

**Question 3: the Browser opens `.wf` files only, since it only views; a `.project` is converted by opening it once in the Designer. Should the Browser convert `.project` files too? I am assuming not.**

## Remove Dependency on AvalonDock

Create an equivalent OptimaxTools.Dock assembly using the 3.8.2 AvalonDock code to develop the same functionality. Fix any problems with AvalonDock code in the process. For now, put OptimaxTools.Dock into WireFlow solution. 

Question: Xceed never published the 3.8.2 source code. 3.8.2 exists only as MS-PL binaries on NuGet. On GitHub, the last source under MS-PL is the "V3.6.0 Released" commit (September 2019). From the "V3.7.0" commit on, the repository's `license.md` is the Xceed Community License (non-commercial), although the file headers still say Ms-PL. Which source should OptimaxTools.Dock start from?
(a) The MS-PL 3.6.0 source. It is clean licensing, but it lacks the 3.7–3.8.1 fixes; I would re-create any we need, comparing behaviour against the 3.8.2 binaries.
(b) A decompilation of the MS-PL 3.8.2 binaries. MS-PL allows derivative works, but the result has no comments, and the XAML has to be recovered from BAML.
(c) Dirkster.AvalonDock. It is an MS-PL fork of Xceed AvalonDock, maintained since 2019, with .NET Core/.NET 8+ builds and many fixes. It is not "the 3.8.2 code", though.
Unless you say otherwise, I will assume (a) when I reach this item.

A: Let's explore the Dirkster option. What license does it have?

Answer: MS-PL, the same license as Xceed AvalonDock 3.6.0 and the one `THIRD-PARTY-NOTICES.md` already carries. It is MS-PL in both places: the repository's `LICENSE` file and the `LICENSE` bundled in its NuGet packages. Dirkster99 forked Xceed's MS-PL source before Xceed switched to the Community License, so the fork never took on the non-commercial terms. It is actively maintained: v5.0.1 was released on 2026-10-04 and targets `net10.0-windows`, `net9.0-windows` and `net48`. The packages are `Dirkster.AvalonDock` plus separate theme packages, such as `Dirkster.AvalonDock.Themes.Metro` and `Dirkster.AvalonDock.Themes.VS2013`. Its namespaces are `AvalonDock.*` rather than `Xceed.Wpf.AvalonDock.*`, so moving to it means changing the Designer's XAML namespaces and `using`s, and the in-repo Metro theme would be replaced by its own Metro theme, with WireFlow's two brush colors carried over.

**Question: Dirkster.AvalonDock gives OptimaxTools.Dock a clean MS-PL starting point with current .NET builds. Should OptimaxTools.Dock be (1) a fork of the Dirkster v5.0.1 source in this solution, renamed to `OptimaxTools.Dock`, so that we can fix problems in it as this item asks, or (2) the `Dirkster.AvalonDock` NuGet package used directly, with no OptimaxTools.Dock assembly? Unless you say otherwise, I will assume (1) when I reach this item.**

# END-OF-TO-DO