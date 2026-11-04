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

