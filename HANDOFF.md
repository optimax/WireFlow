# WireFlow — Handoff

## Current objective

The solution runs on .NET 10 (`net10.0-windows`, WPF), with Newtonsoft.Json 13.0.4 and a `publish-locally.bat` that publishes the Designer. The Designer's Outline view follows changes to a page's elements. Both apps have application icons. No TO-DO items are open.

## Work completed

- All 7 projects are SDK-style, targeting `net10.0-windows`. They keep their assembly names, root namespaces, `Properties\AssemblyInfo.cs` files and WPF `Resource` items.
- NuGet packages are `PackageReference` items: Newtonsoft.Json 13.0.4, Extended.Wpf.Toolkit 2.9.0, Infragistics.Themes.MetroDark.Wpf 1.0.0.
- The Designer and the Metro theme project take only the Xceed DLLs they use from Extended.Wpf.Toolkit (see "Known traps" in `implementation.md`).
- The Metro theme project is signed through `SignAssembly` / `AssemblyOriginatorKeyFile`.
- `Directory.Build.props` stops the SDK from generating the attributes the `AssemblyInfo.cs` files already declare.
- `publish-locally.bat` publishes the 64-bit Designer, built in Release mode, framework-dependent, to `c:\Programs\Optimax\WireFlow`.
- Outline Sync: `DesignerNavigator` raises `ElementsChanged` after it creates, duplicates, deletes, groups, ungroups or reorders elements. `MainDesignerWindow` rebuilds the outline when the event comes from the current designer.
- Application icons: the Designer gets the WF logo as a square icon (white on red), the Browser a variation (red on charcoal with a gray rim). `Documents\make-icons.ps1` generates both at 16, 20, 24, 32, 40, 48, 64 and 256 px.

## Files changed

- All 7 `*.csproj` files — SDK-style. Newtonsoft.Json 13.0.4 in `WireFlow.Repository.Folder` and `WireFlow.Designer`.
- `Xceed.Wpf.AvalonDock.Themes.Metro/Properties/AssemblyInfo.cs` — signing attributes removed.
- `Directory.Build.props`, `publish-locally.bat`, `implementation.md`, `HANDOFF.md` — new.
- `Browser/App.config`, `WireFlow.Designer/app.config`, the three `packages.config` files — deleted.
- `WireFlow.Designer/Classes/DesignerNavigator.cs` — `ElementsChanged` event and `OnElementsChanged()`, raised by `CreateShape` (both overloads), `DuplicateSelectedElements`, `DeleteSelectedElements`, `GroupSelected`, `UngroupSelected`, `ObjectSendBackward`, `ObjectBringForward`, `ObjectSendToBack`, `ObjectBringToFront`.
- `WireFlow.Designer/Views/MainDesignerWindow.xaml.cs` — `NavigatorElementsChangedHandler`, subscribed in `NewPageTab`, `NewLayoutTab`, `NewPartTab`.
- `WireFlow.Designer/Designer.ico`, `Browser/Browser.ico`, `Documents/make-icons.ps1` — new. `WireFlow.Designer.csproj` and `WireFlow.Browser.csproj` — `ApplicationIcon`.

## Build result

Debug: 0 errors, 0 warnings. Earlier builds while Visual Studio had the solution open also showed MSB3026 copy retries caused by file locks; the retries succeeded.

## Test result

- The Browser and the Designer start and stay running. The Designer's main window loads its AvalonDock layout and the Metro theme.
- A script loaded a copy of `Data/WIREFLOW` through `FlatFolderRepository` with Newtonsoft.Json 13.0.4. The project and its `Main` page loaded, and saving produced byte-identical files.
- The Designer starts and responds with the Outline Sync change.
- `PrivateExtractIcons`, which the shell uses, reads 16, 32, 48 and 256 px icons from both built exes. A desktop shortcut was not checked; the shell's icon cache can keep showing an old icon for a while.
- Not tested: interactive use. That includes drawing a shape and watching the Outline update, docking, the property grid, opening a project in the Browser, and following links.

## Unresolved problems

- There is no third-party notices file. `AGENTS.md` asks for one to be updated with the exact license texts whenever a package changes, and the Newtonsoft.Json package ships its `LICENSE.md`.
- `packages/` is in `.gitignore`, but files in it are still tracked. The build does not use that folder.
- Extended.Wpf.Toolkit 3.8.2 is the last MS-PL version (see `implementation.md`). Upgrading from 2.9.0 waits for the user's decision.

## Known regressions

None found.

## Recommended next step

Check Outline Sync interactively: draw, duplicate, group, ungroup, reorder and delete shapes on a page and watch the Outline. Then add the third-party notices file.
