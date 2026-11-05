# WireFlow — Implementation

WireFlow is a tool for building wireframes and clickable prototypes: pages are drawn in the Designer, linked to each other through element hyperlinks (`Href`), and clicked through in the Browser.

## Products

| Product | Project | Role |
|---|---|---|
| WireFlow Designer | `WireFlow.Designer` | WPF editor with a docking UI (OptimaxTools.Dock): pages, layouts (master pages), parts, images, outline, properties. |
| WireFlow Browser | `WireFlow.Browser` | WPF viewer: opens a `.wf` project, shows its `Home` page and follows `Href` clicks with back/forward history. |

Shared projects:

| Project | Role |
|---|---|
| `WireFlow.Types` | Data model (`FlowProject`, `FlowPage`, `FlowLayer`, `FlowElement`, `FlowImage`, `FlowGroup`, `FlowPart`) and the `IFlowRepository` interface. Uses WPF value types such as `FontStretch` and `Stretch`. |
| `WireFlow.Core` | `WireFlowEngine` (renders the model to WPF controls and serializes them back), `DesignerPanel`, outline wrappers, grids. |
| `WireFlow.Repository.Sqlite` | `SqliteRepository`, the storage in use: a project in one SQLite database file (`.wf`). Converts folder projects (`.project`) into `.wf` files. |
| `WireFlow.Repository.Folder` | `FlatFolderRepository`, the earlier storage: a project as a folder. Read when a folder project is converted; its `SerializeToJSON` and `DeserializeFromJSON` are the JSON settings of both storages. |
| `WireFlow.Repository.Archive` | `ZipArchiveRepository`, an unimplemented stub. |
| `OptimaxTools.Dock`, `OptimaxTools.Dock.Core` | The docking layout system of the Designer: the `AvalonDock` and `AvalonDock.Core` components of Dirkster.AvalonDock 5.0.1, an MS-PL fork of Xceed's AvalonDock, renamed to the `OptimaxTools.Dock` namespaces. XAML namespace `http://schemas.optimax.com/wpf/xaml/dock` (prefix `dock`), also mapped into WPF's presentation namespace, as upstream does. Strong-named with WireFlow's `OptimaxTools.Dock\sn.snk`. `LayoutGridControl` gives the splitters between panes the manager's splitter styles only when they are set; upstream set a null `Style`, which hid the theme's splitter style. |
| `OptimaxTools.Dock.Themes.Metro` | The Metro theme of Dirkster.AvalonDock 5.0.1, renamed, used by the Designer, with WireFlow's two brush colors (`BaseColor5`, `BaseColor6`) in `Brushes.xaml`, and in `Theme.xaml` themed backgrounds for the document pane and the Ctrl+Tab navigator window, which upstream draws white, and splitters drawn as a rail with 1 px borders (`AvalonDock_ThemeMetro_SplitterBorder`) inside their 5 px grab area. |

## Development environment

- .NET 10, target framework `net10.0-windows`, WPF (`UseWPF`). All projects are SDK-style.
- Visual Studio 2026 (18.x) with the .NET 10 SDK. The solution file is `WireFlow.slnx`, the XML solution format; it lists the projects only, as they all use the default Debug and Release configurations for Any CPU. ReSharper's settings are in `WireFlow.slnx.DotSettings`.
- NuGet packages are `PackageReference` items restored to the NuGet global packages folder. The `packages/` folder at the root is left over from `packages.config` and is not used by the build.
- Each project declares its assembly identity in `Properties\AssemblyInfo.cs`. `Directory.Build.props` turns off the SDK-generated copies of those attributes, so the SDK only adds the ones the files lack, such as `SupportedOSPlatform`.

## Application icons

`WireFlow.Designer\Designer.ico` (white WF mark on a red tile, as in `Documents\logo.png`) and `Browser\Browser.ico` (red WF mark on a charcoal tile with a gray rim) are the projects' `ApplicationIcon`s. `Documents\make-icons.ps1` draws them on a whole-pixel grid at 16–256 px; change the script and run it again rather than editing the `.ico` files. The Designer's main window sets its own title-bar icon, `Views\logo.png`; the Browser's windows use the application icon.

## Themes

The Designer and the Browser have the themes of Optimax's other programs (PDFTools): View → Theme offers System, Light, Dark and Classic, kept in each program's settings (the Designer's `ConfigSettings.Theme`; the Browser's `BrowserSettings.Theme`, in `%APPDATA%\WireFlow\WireFlow.Browser.config`; anything else is System). They come from `MaxServices.Core` 1.1.0, Optimax's own library made in Optimax.Shared.Lib, kept as a package in `LocalPackages\` and found through `nuget.config`. Its `AppTheme` swaps the first dictionary of `App.xaml` (WPF's Fluent light or dark, or Core's classic) and puts its palette after it.

- `DesignerTheme` applies a choice and keeps the Designer's own brush keys (`ListBackgroundBrush`, `LightLightColorBrush`, `DesignerBackgroundBrush` and the others it lists) and the Metro theme's base colors and splitter border (the palette's `ChromeBorder`, which PDFTools uses for its pane grips) in the application's resources as the palette's brushes for the current theme. XAML uses those keys by `DynamicResource`, and code by `SetResourceReference`, so that a change of theme reaches them.
- The Browser's XAML uses the palette's keys (`Palette.AppBackground`, `Palette.PaneHeader` and others) by `DynamicResource` directly; it has no brush keys of its own to map.
- Neither program has implicit styles for standard controls (menus, list boxes, tree views, splitters): one would hide Fluent's style, and Classic has no Fluent style to base one on. `EditableListBox`, a type of its own that the theme's `ListBox` style does not reach, has one that sets only colors and fonts.
- The page and what is drawn on it keep their own colors; the area around the page follows the theme.
- Fluent's menus and toolbars are taller than the old ones: give the rows that hold them `Auto` height rather than a fixed one.

## Publishing

`publish-locally.bat` publishes the WireFlow Designer, built in Release mode for `win-x64`, to `c:\Programs\Optimax\WireFlow`. The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64). The Browser is not published. The script empties the folder first (see "Known traps"), and stops if it cannot, for example while the published Designer is running.

## Third-party components and licensing

WireFlow itself is copyright Optimax Corporation.

| Component | Version | License | Used by |
|---|---|---|---|
| Newtonsoft.Json | 13.0.4 | MIT | Repository.Folder, Designer, Browser |
| Extended.Wpf.Toolkit (only `Xceed.Wpf.Toolkit`, the property grid) | 3.6.0 | MS-PL | Designer |
| Dirkster.AvalonDock source (as OptimaxTools.Dock, in this solution) | 5.0.1 | MS-PL | Designer |
| Microsoft.Data.Sqlite (with Microsoft.Data.Sqlite.Core) | 10.0.14 | MIT | Repository.Sqlite |
| SQLitePCLRaw (bundle_e_sqlite3, core, lib.e_sqlite3, provider.e_sqlite3) | 2.1.13 | Apache-2.0 | Microsoft.Data.Sqlite |
| SQLite (`e_sqlite3.dll`, in SQLitePCLRaw.lib.e_sqlite3) | as built by SQLitePCLRaw 2.1.13 | Public domain | SQLitePCLRaw |

Extended.Wpf.Toolkit is pinned to 3.6.0 on purpose: it is the last version that is MS-PL both in its package and in its published source, under which WireFlow took it. It ships only `lib/net40` DLLs, and NuGet lists it as unlisted, though it still restores. Do not upgrade it:

- Versions 3.7.0–3.8.1 and 4.0.0 onwards, 5.1.2 included, are under the Xceed Community License: non-commercial use by fewer than 10 users only.
- 3.8.2 declares MS-PL, but the `LICENSE.txt` inside its package is the Community License.
- Xceed never published 3.7+ source under MS-PL: from the 3.7.0 commit on, the repository's `license.md` is the Community License.

Docking is OptimaxTools.Dock, from the source of Dirkster.AvalonDock 5.0.1 (MS-PL in its repository's `LICENSE` and its packages). Keep it close to upstream, so that later upstream fixes can be merged: change only what WireFlow needs, and mark each change with a `WireFlow:` comment.

`THIRD-PARTY-NOTICES.md` holds the license texts. Update it with the exact texts whenever a package is added, removed or upgraded.

## Project file format

A project is one SQLite database file, `<name>.wf`, written by `SqliteRepository`. `PRAGMA application_id` is `0x57464C57` ("WFLW") and `PRAGMA user_version` the format number, now 1; a file with another application id, or a higher format number, is not opened. Format 1 has four tables:

- `Project` — one row: the `FlowProject` JSON (size, background, home page, `RevisionNo`).
- `Documents` — one row per page, layout and part (`Kind` `page`, `layout` or `part`, and `Name`): `Saved`, the JSON of the saved version, and `Draft`, the JSON of the unsaved edits. `SavePage`, `SaveLayout` and `SavePart` write the draft. `SaveProject`, in one transaction, raises `RevisionNo`, archives the project and the saved version of each document that has a draft under the new revision, and makes the drafts the saved versions.
- `Archive` — earlier versions (`Kind`, which can also be `project`, `Name`, `RevisionNo`, JSON): the version that the save of that revision replaced.
- `Images` — imported images (`Name`, the file name, and `Data`).

Names compare ignoring case, as file names did. Each document stays one JSON value rather than a table per element type: the element model is open-ended (`$type` tags), and the programs read and write whole pages. As in the folder storage, a page is read with its draft, but a layout used as a master and a part are read in their saved version only. Renaming a layout or a part is not implemented, since pages and elements refer to them by name, and deleting or renaming a page or layout leaves its archived versions under the old name.

Each operation opens the file and closes it again: pooling is off, so the file is not held open between operations, and the Browser can view a project the Designer is editing. A new or converted file is built under a temporary name (`<name>.wf.tmp`) and then moved into place.

JSON is written by Newtonsoft.Json with `TypeNameHandling.Auto`, so element lists carry `$type` tags. Unknown keys are dropped on load and are therefore lost on the next save.

**Folder projects.** The earlier storage, `FlatFolderRepository`, keeps a project as a folder: `<name>.project` (the `FlowProject` JSON); `pages/*.page`, `layouts/*.master` and `parts/*.part`, with unsaved edits in `*.new` files beside them; `pages.archive/` and the other `*.archive/` folders, holding `<name>.<revision>` files; and `images/`. When the Designer opens a `.project`, `SqliteRepository` converts it into `<name>.wf` beside it, copying the JSON of every file as it is, and opens that; the folder is left as it is. If that `.wf` file already exists, it is opened, not replaced. The Browser opens `.wf` files only and says so when given a `.project`. The sample projects under `Data` are converted that way, each into a `.wf` beside its `.project`; `Data/OPTIMAX.zip` is an earlier format still (`Optimax.wireflow`, elements without `$type` tags), which neither storage reads.

## Known traps

- **Extended.Wpf.Toolkit also ships Xceed's AvalonDock DLLs.** The Designer references the package with `ExcludeAssets="all"` and `GeneratePathProperty="true"`, and takes only `Xceed.Wpf.Toolkit.dll` through `$(PkgExtended_Wpf_Toolkit)`. A NuGet "update" that re-adds every DLL of the package brings Xceed's AvalonDock back beside OptimaxTools.Dock.
- **Inside the `OptimaxTools.*` namespaces, `Dock` is the namespace `OptimaxTools.Dock`, not WPF's `Dock` enum.** C# finds the namespace, a member of `OptimaxTools`, before the `using System.Windows.Controls` of the file, so `Dock.Top` does not compile there, and a file-level `using` alias does not help. Write `System.Windows.Controls.Dock.Top`, as the fork does, when merging upstream code.
- **Scratch programs that host the Designer's window.** Creating `App` queues its `Startup` handler, which opens a real Designer window as soon as the dispatcher runs and applies the saved theme. `MainDesignerWindow` also writes its settings (`%APPDATA%\WireFlow\WireFlow.Designer.config`) when it closes. Remove the `Startup` handler, and keep a copy of the settings file to put back.
- **Single-file `dotnet run app.cs` scripts default to `PublishAot=true`,** which disables dynamic code generation, unlike the apps. Add `#:property PublishAot=false` to scripts that load WireFlow documents, so that Newtonsoft.Json runs as it does in the apps.
- **`dotnet publish` into an existing folder copies a file only when it is newer than the one there.** After a package downgrade, the publish succeeds but keeps the newer DLLs of the old version. Publish into an empty folder, as `publish-locally.bat` does.
- **Visual Studio's design-time builds share `obj/` with command-line builds.** A command-line build while the solution is open in Visual Studio can fail with CS2001 (a missing `*.g.cs`); building again succeeds.
- **Line endings.** The repository has `core.autocrlf=true` and no `.gitattributes`: the index holds LF, the working tree CRLF. Git Bash `sed -i` rewrites a CRLF file as LF; run `unix2dos` on it afterwards.
- **Microsoft.Data.Sqlite 9.x brings SQLitePCLRaw 2.1.10,** whose `lib.e_sqlite3` NuGet flags with a high-severity advisory (NU1903, GHSA-2m69-gcr7-jv3q). Microsoft.Data.Sqlite 10.0.14 brings 2.1.13, which is not flagged. Keep the NuGet audit warnings visible when upgrading it.
- **A part saved by `SavePart` cannot be read back by `GetPart`,** in either storage: the part is serialized without a root `$type` tag, and `GetPart` deserializes it into the abstract `FlowElementBase`, which fails and returns null.
- **`DesignerPanel` lives in Core, but the `DesignerBackgroundBrush` resource exists only in the Designer.** It uses `TryFindResource` so the Browser can host it.
- **The Designer's Outline view is a snapshot.** `MainDesignerWindow.PopulateOutlineTree` rebuilds it on navigation, on a tab switch, and when the current `DesignerNavigator` raises `ElementsChanged`. A new `DesignerNavigator` operation that adds, removes or reorders elements must call `OnElementsChanged()`, or the Outline goes stale.
- **The Xceed `PropertyGrid` template hard-codes light colors.** Its property list uses `x:Static` system brushes, its filter box `White`, and its category headers an `Expander` style with `#FFF0F0F0`, so no resource or implicit style reaches them, and each category's `GroupItem` gets an explicit style from the template's `CategoryGroupStyleSelector`. `MainDesignerWindow.PropertyGridLoadedHandler` sets local values on those template parts and swaps in a `CategoryGroupStyleSelector` whose `ItemGroupStyle` is `PropertyCategoryStyle` from the grid's resources.
- **A WPF `ListBox` reports a cleared selection as `SelectedIndex` -1.** Code that indexes `Items` with it must check the range.
- **.NET Framework's `System.Drawing.Icon` skips PNG-compressed icon images,** such as the 256 px images of the application icons, so Windows PowerShell cannot load them that way. Check a built exe with the shell's `PrivateExtractIcons` instead.
- **Old sample pages without `$type` tags** (the drafts in `Data/OPTIMAX.zip`) cannot be deserialized into the abstract `FlowElementBase`.
