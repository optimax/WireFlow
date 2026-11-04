---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Properties Pane Styles

 ![pasted-2](<TO-DO.images/pasted-2.png>)

The Properties pane has a white background in what is a dark Theme which is jarring. 

Done: the property list, the empty area below it, the category headers, the text editors and the filter box now use the pane's dark colors. The toolkit's template hard-codes those light colors, so the Designer overrides them from code when the grid loads (see "Known traps" in `implementation.md`). Checked in off-screen renders of the real Designer window, with and without a selected element; the checkbox and the scrollbar keep the standard WPF look, as elsewhere in the Designer. The Fluent themes item below will revisit these colors.

## Convert to WPF Fluent Themes

Convert the application chrome to WPF Fluent themes, with the selection of Light, Dark, System, and None/Classic (like PDFTools and other Optimax apps). Make the theme selection available under View|Theme menu (place View after Edit).

## Convert Data to SQLite

Convert the WireFlow project files and subordinate folders into SQLite databases, with `.wf` extension. Figure out appropriate schema to support the functionality. Convert both Designer and Browser to use `.wf` files.

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