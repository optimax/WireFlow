---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

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

## [>>WORKING<<] Properties Pane Styles

 ![pasted-2](<TO-DO.images/pasted-2.png>)

The Properties pane has a white background in what is a dark Theme which is jarring. 

## Convert to WPF Fluent Themes

Convert the application chrome to WPF Fluent themes, with the selection of Light, Dark, System, and None/Classic (like PDFTools and other Optimax apps). Make the theme selection available under View|Theme menu (place View after Edit).

## Convert Data to SQLite

Convert the WireFlow project files and subordinate folders into SQLite databases, with `.wf` extension. Figure out appropriate schema to support the functionality. Convert both Designer and Browser to use `.wf` files.

## Remove Dependency on AvalonDock

Create an equivalent OptimaxTools.Dock assembly using the 3.8.2 AvalonDock code to develop the same functionality. Fix any problems with AvalonDock code in the process. For now, put OptimaxTools.Dock into WireFlow solution. 

**Question: Xceed never published the 3.8.2 source code. 3.8.2 exists only as MS-PL binaries on NuGet. On GitHub, the last source under MS-PL is the "V3.6.0 Released" commit (September 2019). From the "V3.7.0" commit on, the repository's `license.md` is the Xceed Community License (non-commercial), although the file headers still say Ms-PL. Which source should OptimaxTools.Dock start from?**
**(a) The MS-PL 3.6.0 source. It is clean licensing, but it lacks the 3.7–3.8.1 fixes; I would re-create any we need, comparing behaviour against the 3.8.2 binaries.**
**(b) A decompilation of the MS-PL 3.8.2 binaries. MS-PL allows derivative works, but the result has no comments, and the XAML has to be recovered from BAML.**
**(c) Dirkster.AvalonDock. It is an MS-PL fork of Xceed AvalonDock, maintained since 2019, with .NET Core/.NET 8+ builds and many fixes. It is not "the 3.8.2 code", though.**
**Unless you say otherwise, I will assume (a) when I reach this item.**

A: Let's explore the Dirkster option. What license does it have?

# END-OF-TO-DO