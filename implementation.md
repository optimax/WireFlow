# WireFlow — Implementation

WireFlow is a tool for building wireframes and clickable prototypes: pages are drawn in the Designer, linked to each other through element hyperlinks (`Href`), and clicked through in the Browser.

## Products

| Product | Project | Role |
|---|---|---|
| WireFlow Designer | `WireFlow.Designer` | WPF editor with a docking UI (AvalonDock): pages, layouts (master pages), parts, images, outline, properties. |
| WireFlow Browser | `WireFlow.Browser` | WPF viewer: opens a `.project`, shows its `Home` page and follows `Href` clicks with back/forward history. |

Shared projects:

| Project | Role |
|---|---|
| `WireFlow.Types` | Data model (`FlowProject`, `FlowPage`, `FlowLayer`, `FlowElement`, `FlowImage`, `FlowGroup`, `FlowPart`) and the `IFlowRepository` interface. Uses WPF value types such as `FontStretch` and `Stretch`. |
| `WireFlow.Core` | `WireFlowEngine` (renders the model to WPF controls and serializes them back), `DesignerPanel`, outline wrappers, grids. |
| `WireFlow.Repository.Folder` | `FlatFolderRepository`, the storage in use. |
| `WireFlow.Repository.Archive` | `ZipArchiveRepository`, an unimplemented stub. |
| `Xceed.Wpf.AvalonDock.Themes.Metro` | Source copy of the Extended WPF Toolkit 2.9 AvalonDock Metro theme, used by the Designer. Strong-named with `sn.snk`. |

## Development environment

- .NET 10, target framework `net10.0-windows`, WPF (`UseWPF`). All projects are SDK-style.
- Visual Studio 2026 (18.x) with the .NET 10 SDK. The solution file is the classic `.sln`.
- NuGet packages are `PackageReference` items restored to the NuGet global packages folder. The `packages/` folder at the root is left over from `packages.config` and is not used by the build.
- Each project declares its assembly identity in `Properties\AssemblyInfo.cs`. `Directory.Build.props` turns off the SDK-generated copies of those attributes, so the SDK only adds the ones the files lack, such as `SupportedOSPlatform`.

## Publishing

`publish-locally.bat` publishes the WireFlow Designer, built in Release mode for `win-x64`, to `c:\Programs\Optimax\WireFlow`. The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64). The Browser is not published.

## Third-party components and licensing

WireFlow itself is copyright Optimax Corporation.

| Component | Version | License | Used by |
|---|---|---|---|
| Newtonsoft.Json | 13.0.4 | MIT | Repository.Folder, Designer |
| Extended.Wpf.Toolkit (Xceed AvalonDock, Xceed Toolkit) | 2.9.0 | MS-PL | Designer, Metro theme |
| Infragistics.Themes.MetroDark.Wpf (also supplies `WPFToolkit.dll` and the `System.Windows.Controls.*.Toolkit` DLLs) | 1.0.0 | Not declared in the package | Browser |

Extended.Wpf.Toolkit is pinned to 2.9.0 on purpose. Version 5.1.2 is under the Xceed Community License: non-commercial use by fewer than 10 users only. Check the license of any version above 2.9.0 before upgrading.

## Project file format

A project is a folder:

- `<name>.project` — `FlowProject` JSON (size, background, home page).
- `pages/*.page`, `layouts/*.master`, `parts/*.part` — JSON documents. Unsaved edits are written to `*.new` files beside them; `SaveProject` archives the previous version into `pages.archive/` (and the other `*.archive/` folders) under the project's `RevisionNo`, then promotes the `.new` files.
- `images/` — imported images.

JSON is written by Newtonsoft.Json with `TypeNameHandling.Auto`, so element lists carry `$type` tags. Unknown keys are dropped on load and are therefore lost on the next save.

## Known traps

- **Two `Xceed.Wpf.AvalonDock.Themes.Metro.dll`.** The Extended.Wpf.Toolkit package ships its own Metro theme DLL with the same assembly name as the project in this solution. The Designer and the Metro theme project therefore reference the package with `ExcludeAssets="all"` and `GeneratePathProperty="true"`, and take only the DLLs they use through `$(PkgExtended_Wpf_Toolkit)`. A NuGet "update" that re-adds every DLL of the package brings the clash back.
- **Single-file `dotnet run app.cs` scripts default to `PublishAot=true`,** which disables dynamic code generation, unlike the apps. Add `#:property PublishAot=false` to scripts that load WireFlow documents, so that Newtonsoft.Json runs as it does in the apps.
- **Visual Studio's design-time builds share `obj/` with command-line builds.** A command-line build while the solution is open in Visual Studio can fail with CS2001 (a missing `*.g.cs`); building again succeeds.
- **Line endings.** The repository has `core.autocrlf=true` and no `.gitattributes`: the index holds LF, the working tree CRLF. Git Bash `sed -i` rewrites a CRLF file as LF; run `unix2dos` on it afterwards.
- **`DesignerPanel` lives in Core, but the `DesignerBackgroundBrush` resource exists only in the Designer.** It uses `TryFindResource` so the Browser can host it.
- **The Designer's Outline view is a snapshot.** `MainDesignerWindow.PopulateOutlineTree` rebuilds it on navigation, on a tab switch, and when the current `DesignerNavigator` raises `ElementsChanged`. A new `DesignerNavigator` operation that adds, removes or reorders elements must call `OnElementsChanged()`, or the Outline goes stale.
- **A WPF `ListBox` reports a cleared selection as `SelectedIndex` -1.** Code that indexes `Items` with it must check the range.
- **Old sample pages without `$type` tags** (the drafts in `Data/OPTIMAX.zip`) cannot be deserialized into the abstract `FlowElementBase`.
