# WireFlow — Handoff

## Current objective

The solution runs on .NET 10 (`net10.0-windows`, WPF), with Newtonsoft.Json 13.0.4 and a `publish-locally.bat` that publishes the Designer. The next TO-DO item is "Outline Sync".

## Work completed

- All 7 projects are SDK-style, targeting `net10.0-windows`. They keep their assembly names, root namespaces, `Properties\AssemblyInfo.cs` files and WPF `Resource` items.
- NuGet packages are `PackageReference` items: Newtonsoft.Json 13.0.4, Extended.Wpf.Toolkit 2.9.0, Infragistics.Themes.MetroDark.Wpf 1.0.0.
- The Designer and the Metro theme project take only the Xceed DLLs they use from Extended.Wpf.Toolkit (see "Known traps" in `implementation.md`).
- The Metro theme project is signed through `SignAssembly` / `AssemblyOriginatorKeyFile`.
- `Directory.Build.props` stops the SDK from generating the attributes the `AssemblyInfo.cs` files already declare.
- `publish-locally.bat` publishes the 64-bit Designer, built in Release mode, framework-dependent, to `c:\Programs\Optimax\WireFlow`.

## Files changed

- All 7 `*.csproj` files — SDK-style. Newtonsoft.Json 13.0.4 in `WireFlow.Repository.Folder` and `WireFlow.Designer`.
- `Xceed.Wpf.AvalonDock.Themes.Metro/Properties/AssemblyInfo.cs` — signing attributes removed.
- `Directory.Build.props`, `publish-locally.bat`, `implementation.md`, `HANDOFF.md` — new.
- `Browser/App.config`, `WireFlow.Designer/app.config`, the three `packages.config` files — deleted.

## Build result

Debug: 0 errors. The only warnings were MSB3026 copy retries caused by file locks while Visual Studio had the solution open; the retries succeeded.

## Test result

- The Browser and the Designer start and stay running. The Designer's main window loads its AvalonDock layout and the Metro theme.
- A script loaded a copy of `Data/WIREFLOW` through `FlatFolderRepository` with Newtonsoft.Json 13.0.4. The project and its `Main` page loaded, and saving produced byte-identical files.
- Not tested: interactive use. That includes drawing, docking, the property grid, opening a project in the Browser, and following links.

## Unresolved problems

- There is no third-party notices file. `AGENTS.md` asks for one to be updated with the exact license texts whenever a package changes, and the Newtonsoft.Json package ships its `LICENSE.md`.
- `packages/` is in `.gitignore`, but files in it are still tracked. The build does not use that folder.

## Known regressions

None found.

## Recommended next step

The "Outline Sync" TO-DO item: a new element, such as a rectangle, does not appear in the Outline view.
