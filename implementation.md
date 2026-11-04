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
| `Xceed.Wpf.AvalonDock.Themes.Metro` | Source copy of the Extended WPF Toolkit 3.6.0 AvalonDock Metro theme, used by the Designer, with two brush colors in `Brushes.xaml` changed for WireFlow and a themed background for the document pane in `Theme.xaml`. 3.6.0 is the last theme source Xceed published under MS-PL; the 3.8 theme differs from it only by one template moved into a keyed resource. Strong-named with `sn.snk`. |

## Development environment

- .NET 10, target framework `net10.0-windows`, WPF (`UseWPF`). All projects are SDK-style.
- Visual Studio 2026 (18.x) with the .NET 10 SDK. The solution file is `WireFlow.slnx`, the XML solution format; it lists the projects only, as they all use the default Debug and Release configurations for Any CPU. ReSharper's settings are in `WireFlow.slnx.DotSettings`.
- NuGet packages are `PackageReference` items restored to the NuGet global packages folder. The `packages/` folder at the root is left over from `packages.config` and is not used by the build.
- Each project declares its assembly identity in `Properties\AssemblyInfo.cs`. `Directory.Build.props` turns off the SDK-generated copies of those attributes, so the SDK only adds the ones the files lack, such as `SupportedOSPlatform`.

## Application icons

`WireFlow.Designer\Designer.ico` (white WF mark on a red tile, as in `Documents\logo.png`) and `Browser\Browser.ico` (red WF mark on a charcoal tile with a gray rim, after the Browser's MetroDark theme) are the projects' `ApplicationIcon`s. `Documents\make-icons.ps1` draws them on a whole-pixel grid at 16–256 px; change the script and run it again rather than editing the `.ico` files. The Designer's main window sets its own title-bar icon, `Views\logo.png`; the Browser's windows use the application icon.

## Themes

The Designer has the themes of Optimax's other programs (PDFTools): View → Theme offers System, Light, Dark and Classic, kept in its settings (`ConfigSettings.Theme`; anything else is System). They come from `MaxServices.Core` 1.1.0, Optimax's own library made in Optimax.Shared.Lib, kept as a package in `LocalPackages\` and found through `nuget.config`. Its `AppTheme` swaps the first dictionary of `App.xaml` (WPF's Fluent light or dark, or Core's classic) and puts its palette after it.

- `DesignerTheme` applies a choice and keeps the Designer's own brush keys (`ListBackgroundBrush`, `LightLightColorBrush`, `DesignerBackgroundBrush` and the others it lists) and the Metro theme's base colors in the application's resources as the palette's brushes for the current theme. XAML uses those keys by `DynamicResource`, and code by `SetResourceReference`, so that a change of theme reaches them.
- The Designer has no implicit styles for standard controls (menus, list boxes, tree views, splitters): one would hide Fluent's style, and Classic has no Fluent style to base one on. `EditableListBox`, a type of its own that the theme's `ListBox` style does not reach, has one that sets only colors and fonts.
- The page and what is drawn on it keep their own colors; the area around the page follows the theme.
- The Browser keeps the Infragistics MetroDark theme (see `TO-DO.md`).

## Publishing

`publish-locally.bat` publishes the WireFlow Designer, built in Release mode for `win-x64`, to `c:\Programs\Optimax\WireFlow`. The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64). The Browser is not published. The script empties the folder first (see "Known traps"), and stops if it cannot, for example while the published Designer is running.

## Third-party components and licensing

WireFlow itself is copyright Optimax Corporation.

| Component | Version | License | Used by |
|---|---|---|---|
| Newtonsoft.Json | 13.0.4 | MIT | Repository.Folder, Designer |
| Extended.Wpf.Toolkit (Xceed AvalonDock, Xceed Toolkit) | 3.6.0 | MS-PL | Designer, Metro theme |
| Infragistics.Themes.MetroDark.Wpf (also supplies `WPFToolkit.dll` and the `System.Windows.Controls.*.Toolkit` DLLs) | 1.0.0 | Not declared in the package | Browser |

Extended.Wpf.Toolkit is pinned to 3.6.0 on purpose: it is the last version that is MS-PL both in its package and in its published source, which matches the `Xceed.Wpf.AvalonDock.Themes.Metro` source in this solution. It ships only `lib/net40` DLLs, and NuGet lists it as unlisted, though it still restores. Do not upgrade it:

- Versions 3.7.0–3.8.1 and 4.0.0 onwards, 5.1.2 included, are under the Xceed Community License: non-commercial use by fewer than 10 users only.
- 3.8.2 declares MS-PL, but the `LICENSE.txt` inside its package is the Community License.
- Xceed never published 3.7+ source under MS-PL: from the 3.7.0 commit on, the repository's `license.md` is the Community License.

AvalonDock is to be replaced by an OptimaxTools.Dock assembly in this solution (see `TO-DO.md`).

`THIRD-PARTY-NOTICES.md` holds the license texts. Update it with the exact texts whenever a package is added, removed or upgraded.

## Project file format

A project is a folder:

- `<name>.project` — `FlowProject` JSON (size, background, home page).
- `pages/*.page`, `layouts/*.master`, `parts/*.part` — JSON documents. Unsaved edits are written to `*.new` files beside them; `SaveProject` archives the previous version into `pages.archive/` (and the other `*.archive/` folders) under the project's `RevisionNo`, then promotes the `.new` files.
- `images/` — imported images.

JSON is written by Newtonsoft.Json with `TypeNameHandling.Auto`, so element lists carry `$type` tags. Unknown keys are dropped on load and are therefore lost on the next save.

## Known traps

- **Two `Xceed.Wpf.AvalonDock.Themes.Metro.dll`.** The Extended.Wpf.Toolkit package ships its own Metro theme DLL with the same assembly name as the project in this solution. The Designer and the Metro theme project therefore reference the package with `ExcludeAssets="all"` and `GeneratePathProperty="true"`, and take only the DLLs they use through `$(PkgExtended_Wpf_Toolkit)`. A NuGet "update" that re-adds every DLL of the package brings the clash back.
- **Scratch programs that host the Designer's window.** Creating `App` queues its `Startup` handler, which opens a real Designer window as soon as the dispatcher runs and applies the saved theme. `MainDesignerWindow` also writes its settings (`%APPDATA%\WireFlow\WireFlow.Designer.config`) when it closes. Remove the `Startup` handler, and keep a copy of the settings file to put back.
- **Single-file `dotnet run app.cs` scripts default to `PublishAot=true`,** which disables dynamic code generation, unlike the apps. Add `#:property PublishAot=false` to scripts that load WireFlow documents, so that Newtonsoft.Json runs as it does in the apps.
- **`dotnet publish` into an existing folder copies a file only when it is newer than the one there.** After a package downgrade, the publish succeeds but keeps the newer DLLs of the old version. Publish into an empty folder, as `publish-locally.bat` does.
- **Visual Studio's design-time builds share `obj/` with command-line builds.** A command-line build while the solution is open in Visual Studio can fail with CS2001 (a missing `*.g.cs`); building again succeeds.
- **Line endings.** The repository has `core.autocrlf=true` and no `.gitattributes`: the index holds LF, the working tree CRLF. Git Bash `sed -i` rewrites a CRLF file as LF; run `unix2dos` on it afterwards.
- **`DesignerPanel` lives in Core, but the `DesignerBackgroundBrush` resource exists only in the Designer.** It uses `TryFindResource` so the Browser can host it.
- **The Designer's Outline view is a snapshot.** `MainDesignerWindow.PopulateOutlineTree` rebuilds it on navigation, on a tab switch, and when the current `DesignerNavigator` raises `ElementsChanged`. A new `DesignerNavigator` operation that adds, removes or reorders elements must call `OnElementsChanged()`, or the Outline goes stale.
- **The Xceed `PropertyGrid` template hard-codes light colors.** Its property list uses `x:Static` system brushes, its filter box `White`, and its category headers an `Expander` style with `#FFF0F0F0`, so no resource or implicit style reaches them, and each category's `GroupItem` gets an explicit style from the template's `CategoryGroupStyleSelector`. `MainDesignerWindow.PropertyGridLoadedHandler` sets local values on those template parts and swaps in a `CategoryGroupStyleSelector` whose `ItemGroupStyle` is `PropertyCategoryStyle` from the grid's resources.
- **A WPF `ListBox` reports a cleared selection as `SelectedIndex` -1.** Code that indexes `Items` with it must check the range.
- **.NET Framework's `System.Drawing.Icon` skips PNG-compressed icon images,** such as the 256 px images of the application icons, so Windows PowerShell cannot load them that way. Check a built exe with the shell's `PrivateExtractIcons` instead.
- **Old sample pages without `$type` tags** (the drafts in `Data/OPTIMAX.zip`) cannot be deserialized into the abstract `FlowElementBase`.
