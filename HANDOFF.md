# WireFlow — Handoff

## Current objective

The solution has been converted from .NET Framework 4.8 to .NET 10 (`net10.0-windows`, WPF). Two questions on that TO-DO item are open in `TO-DO.md`: upgrading Newtonsoft.Json, and the missing `publish-locally.bat`.

## Work completed

- All 7 projects are SDK-style, targeting `net10.0-windows`. They keep their assembly names, root namespaces, `Properties\AssemblyInfo.cs` files and WPF `Resource` items.
- `packages.config` files are replaced by `PackageReference`. The package versions are unchanged: Newtonsoft.Json 8.0.3, Extended.Wpf.Toolkit 2.9.0, Infragistics.Themes.MetroDark.Wpf 1.0.0.
- The Designer and the Metro theme project take only the Xceed DLLs they use from Extended.Wpf.Toolkit (see "Known traps" in `implementation.md`).
- The Metro theme project is signed through `SignAssembly` / `AssemblyOriginatorKeyFile` instead of an `AssemblyKeyFile` attribute, whose `..\..\sn.snk` path only worked from the old `obj\Debug` folder.
- `Directory.Build.props` stops the SDK from generating the attributes the `AssemblyInfo.cs` files already declare. The SDK still adds `SupportedOSPlatform`, which keeps analyzer CA1416 quiet about the registry calls in `RecentFileList`.
- The unused `WPFToolkit` reference in `WireFlow.Types` was dropped.
- `App.config` / `app.config` (which only held the .NET Framework `supportedRuntime`) and the `packages.config` files were deleted.
- The root `implementation.md` was created.

## Files changed

- `Browser/WireFlow.Browser.csproj`, `WireFlow.Core/WireFlow.Core.csproj`, `WireFlow.Designer/WireFlow.Designer.csproj`, `WireFlow.Repository.Archive/WireFlow.Repository.Archive.csproj`, `WireFlow.Repository.Folder/WireFlow.Repository.Folder.csproj`, `WireFlow.Types/WireFlow.Types.csproj`, `Xceed.Wpf.AvalonDock.Themes.Metro/Xceed.Wpf.AvalonDock.Themes.Metro.csproj` — rewritten as SDK-style.
- `Xceed.Wpf.AvalonDock.Themes.Metro/Properties/AssemblyInfo.cs` — signing attributes removed.
- `Directory.Build.props`, `implementation.md`, `HANDOFF.md` — new.
- `Browser/App.config`, `WireFlow.Designer/app.config`, `Browser/packages.config`, `WireFlow.Designer/packages.config`, `WireFlow.Repository.Folder/packages.config` — deleted.
- `TO-DO.md` — task status and questions.

## Build result

- Debug and Release: 0 errors, 8 warnings. All the warnings are NU1701 for Newtonsoft.Json 8.0.3 (NuGet falls back to its .NET Framework DLL).
- A restore that can reach nuget.org also reports NU1903, a known high-severity vulnerability in Newtonsoft.Json 8.0.3 (GHSA-5crp-9r3c-p9vr).

## Test result

- The Browser and the Designer (Debug) start and stay running. The Designer's main window loads its AvalonDock layout and the Metro theme.
- A script loaded a copy of `Data/WIREFLOW` through `FlatFolderRepository` on .NET 10. The project and its `Main` page loaded, and saving the page again produced a byte-identical file.
- Not tested: interactive use. That includes drawing, docking, the property grid, opening a project in the Browser, and following links.

## Unresolved problems

- Newtonsoft.Json 8.0.3: the fallback DLL and the vulnerability (question 1 in `TO-DO.md`).
- `publish-locally.bat`, required by `TO-DO-INSTRUCTIONS.md`, does not exist (question 2 in `TO-DO.md`).
- `packages/` is in `.gitignore`, but 43 files in it are still tracked. The build no longer uses that folder.

## Known regressions

None found.

## Recommended next step

Open the solution in Visual Studio (reload the projects when prompted), and exercise the Designer and the Browser interactively. Then answer the two questions in `TO-DO.md`.
