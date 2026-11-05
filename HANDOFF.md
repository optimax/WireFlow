# WireFlow — Handoff

## Current objective

Processing `TO-DO.md` top to bottom. Done: the `.slnx` conversion, AvalonDock (Extended.Wpf.Toolkit) 3.6.0, the dark Properties pane, the Fluent themes in the Designer and the Browser, `.wf` (SQLite) project files, OptimaxTools.Dock, the fork of Dirkster.AvalonDock 5.0.1 that replaces Xceed's AvalonDock, and splitters with visible borders, the sample projects in `Data` as `.wf` files only, the 2016 OPTIMAX project migrated into `Data/optimax-old.wf`, a frame around the page in both programs, a gray default page background, and compact Tools rows. Next: "XML vs JSON" in `TO-DO.md`.

## Work completed

- All 7 projects are SDK-style, targeting `net10.0-windows`. They keep their assembly names, root namespaces, `Properties\AssemblyInfo.cs` files and WPF `Resource` items. The solution file is `WireFlow.slnx`.
- NuGet packages are `PackageReference` items: Newtonsoft.Json 13.0.4, Extended.Wpf.Toolkit 3.6.0, Microsoft.Data.Sqlite 10.0.14, and Optimax's own MaxServices.Core 1.1.0 from `LocalPackages\`. `THIRD-PARTY-NOTICES.md` holds the third-party license texts.
- The Designer takes only `Xceed.Wpf.Toolkit.dll` (the property grid) from Extended.Wpf.Toolkit (see "Known traps" in `implementation.md`).
- Docking: `OptimaxTools.Dock`, `OptimaxTools.Dock.Core` and `OptimaxTools.Dock.Themes.Metro` are the renamed source of Dirkster.AvalonDock 5.0.1 (see the shared projects in `implementation.md`).
- Tools list: its rows are 27 px tall in the Fluent themes, as a menu item, rather than 45 px, through a local `Padding` on each row; Classic keeps its own 23 px rows (see "Themes" in `implementation.md`).
- Default page background: a new project's pages are `#555` (`FlowProject.DEFAULT_BACKGROUND`), instead of `#333`, which is the area around the page in Dark. Existing projects keep the background they have.
- Page frame: `DesignerPanel` draws a 1 px frame in the palette's `PageFrame` just outside the page, at its zoomed and panned bounds, so the page shows when its color is the theme's `PreviewBackground` (`#333333` in Dark, the samples' page color).
- Splitters: the splitters between panes are a rail with 1 px borders in the palette's `ChromeBorder`, inside their 5 px grab area. They now get the Metro theme's splitter style at all: `LayoutGridControl.CreateSplitters` used to give them a null `Style`.
- `Directory.Build.props` stops the SDK from generating the attributes the `AssemblyInfo.cs` files already declare.
- `publish-locally.bat` publishes the 64-bit Designer, built in Release mode, framework-dependent, to `c:\Programs\Optimax\WireFlow`.
- Outline Sync: `DesignerNavigator` raises `ElementsChanged` after it creates, duplicates, deletes, groups, ungroups or reorders elements. `MainDesignerWindow` rebuilds the outline when the event comes from the current designer.
- Application icons: the Designer has the WF logo as a square icon (white on red), the Browser a variation (red on charcoal with a gray rim). `Documents\make-icons.ps1` generates both at 16, 20, 24, 32, 40, 48, 64 and 256 px.
- Properties pane: `MainDesignerWindow.PropertyGridLoadedHandler` gives the PropertyGrid's list, filter box and category headers the pane's dark colors; `PropertyCategoryStyle` and the editor text box style are in the grid's resources in `MainDesignerWindow.xaml`.
- `publish-locally.bat` empties the publish folder before publishing.
- Themes: the Designer has View → Theme (System, Light, Dark, Classic) through `MaxServices.Core` 1.1.0's `AppTheme`, as in PDFTools; `DesignerTheme` maps the Designer's brush keys and the Metro theme's base colors to the palette (see "Themes" in `implementation.md`).
- `.wf` project files: `SqliteRepository` (new `WireFlow.Repository.Sqlite` project) keeps a project in one SQLite file and is the storage of both programs; the Designer converts a `.project` it opens into a `.wf` beside it (see "Project file format" in `implementation.md`).
- Browser themes: the same View → Theme menu, kept in `BrowserSettings` (`%APPDATA%\WireFlow\WireFlow.Browser.config`). The Infragistics MetroDark theme and package are gone.

## Files changed

- All 7 `*.csproj` files — SDK-style. Newtonsoft.Json 13.0.4 in `WireFlow.Repository.Folder` and `WireFlow.Designer`; Extended.Wpf.Toolkit 3.6.0 in `WireFlow.Designer` and `Xceed.Wpf.AvalonDock.Themes.Metro`; `ApplicationIcon` in `WireFlow.Designer` and `WireFlow.Browser`.
- OptimaxTools.Dock: `OptimaxTools.Dock/`, `OptimaxTools.Dock.Core/`, `OptimaxTools.Dock.Themes.Metro/` (new: the renamed upstream source, project files, `LICENSE`, identity attributes in `Properties/AssemblyInfo.cs`; `ToggleDockingManager.cs` writes WPF's `Dock` enum in full; the theme's `Brushes.xaml` and `Theme.xaml` carry WireFlow's changes); `Xceed.Wpf.AvalonDock.Themes.Metro/` (deleted); `WireFlow.slnx`; `WireFlow.Designer/WireFlow.Designer.csproj` (the fork instead of Xceed's AvalonDock); `App.xaml` (the fork's Metro theme); `DesignerNavigatorDocument.cs`, `MainDesignerWindow.xaml.cs` (`using OptimaxTools.Dock.Layout`); `MainDesignerWindow.xaml`, `Xaml/GlobalStyles.xaml`, `Classes/DesignerContextMenu.xaml` (the `dock` XAML namespace).
- `Directory.Build.props`, `publish-locally.bat`, `implementation.md`, `HANDOFF.md`, `THIRD-PARTY-NOTICES.md` — new.
- `Browser/App.config`, `WireFlow.Designer/app.config`, the three `packages.config` files — deleted.
- `WireFlow.Designer/Classes/DesignerNavigator.cs` — `ElementsChanged` event and `OnElementsChanged()`, raised by `CreateShape` (both overloads), `DuplicateSelectedElements`, `DeleteSelectedElements`, `GroupSelected`, `UngroupSelected`, `ObjectSendBackward`, `ObjectBringForward`, `ObjectSendToBack`, `ObjectBringToFront`.
- `WireFlow.Designer/Views/MainDesignerWindow.xaml.cs` — `NavigatorElementsChangedHandler`, subscribed in `NewPageTab`, `NewLayoutTab`, `NewPartTab`; `PropertyGridLoadedHandler`, subscribed in the constructor.
- `WireFlow.Designer/Views/MainDesignerWindow.xaml` — PropertyGrid resources: `PropertyCategoryExpanderTemplate`, `PropertyCategoryStyle`, and a `PropertyGridEditorTextBox` style; the View → Theme menu; themed brushes by `DynamicResource`.
- Themes: `nuget.config` and `LocalPackages/MaxServices.Core.1.1.0.nupkg` (new); `WireFlow.Designer/Classes/DesignerTheme.cs` (new); `App.xaml` (Fluent and Core's styles first); `Xaml/GlobalStyles.xaml` (implicit control styles removed, themed brushes moved to `DesignerTheme`); `ConfigSettings.cs` (`Theme`); `MainDesignerWindow.xaml.cs` (`LoadConfigSettings` in the constructor, `ThemeMenuItem_Click`, `UpdateThemeMenu`, title bar); the three dialogs (title bar); `DesignerNavigator.cs` and `WireFlow.Core/DesignerPanel.cs` (`DesignerBackgroundBrush` by reference); the Metro theme's `Theme.xaml` (document pane background).
- Browser themes: `Browser/BrowserSettings.cs` (new); `Browser/App.xaml` (Fluent and Core's styles); `Browser/MainWindow.xaml` (View → Theme, palette brushes, menu row `Auto`); `Browser/MainWindow.xaml.cs` (settings, theme applied before `InitializeComponent`, `ThemeMenuItem_Click`, `UpdateThemeMenu`, title bar); `Browser/WireFlow.Browser.csproj` (MaxServices.Core and Newtonsoft.Json in, Infragistics out); `Browser/Themes/MetroDark/` — deleted.
- `.wf` files: `WireFlow.Repository.Sqlite/` (new: `SqliteRepository.cs`, `Properties/AssemblyInfo.cs`, the `.csproj`); `WireFlow.slnx`; `WireFlow.Designer/WireFlow.Designer.csproj` and `Browser/WireFlow.Browser.csproj` (reference it instead of the folder storage); `WireFlow.Designer/App.xaml.cs` (`SqliteRepository` with `ConvertFolderProjects`); `MainDesignerWindow.xaml.cs` (`NewProject` takes the name from the repository, `OpenProject` lists the `.wf` in the recent files, `OpenProjectFile` reports a failed open, `DeleteSelectedPart` calls `DeletePart`); `Browser/MainWindow.xaml.cs` (`SqliteRepository`, `OpenDocumentFile` reports a `.project` or a failed open).
- `WireFlow.Designer/Designer.ico`, `Browser/Browser.ico`, `Documents/make-icons.ps1` — new.
- `Data/MarketMonitor.wf`, `Data/Optimax.wf`, `Data/Test.wf`, `Data/Test2.wf`, `Data/WireFlow.wf` — converted from the folder projects, which are removed with `Data/OPTIMAX.zip` and the empty folders; `Data/optimax-old.wf` — new, migrated from that zip.
- Tools list: `WireFlow.Designer/Views/MainDesignerWindow.xaml.cs` (constructor; `ToolsItemPadding` and `ToolsContainersChangedHandler`, new).
- Default page background: `WireFlow.Types/RepositoryTypes/FlowProject.cs` (`DEFAULT_BACKGROUND`, the constructor); `WireFlow.Repository.Sqlite/SqliteRepository.cs`, `WireFlow.Repository.Folder/FlatFolderRepository.cs` (`DefaultBackground`); `WireFlow.Core/Constants.cs` (`DEFAULT_BACKGROUND`, was `#111`).
- Page frame: `WireFlow.Core/DesignerPanel.cs` (constructor; `PageFrameBrush` and `OnRender`, new); `WireFlow.Designer/Classes/DesignerTheme.cs` (`PageFrameBrush` maps to `PageFrame`); `Browser/MainWindow.xaml` (the viewer's `PageFrameBrush`).
- Splitters: `OptimaxTools.Dock/Controls/LayoutGridControl.cs` (`CreateSplitters`); `OptimaxTools.Dock.Themes.Metro/Theme.xaml` (the `LayoutGridResizerControl` template) and `Brushes.xaml` (`AvalonDock_ThemeMetro_SplitterBorder`); `WireFlow.Designer/Classes/DesignerTheme.cs` (maps it to `ChromeBorder`).
- `WireFlow.slnx` — replaces `WireFlow.sln` (generated by `dotnet sln migrate`). `WireFlow.sln.DotSettings` renamed to `WireFlow.slnx.DotSettings`.

## Build result

Debug and Release: 0 errors, 0 warnings, with Extended.Wpf.Toolkit 3.6.0. Visual Studio's MSBuild and `dotnet build` both build `WireFlow.slnx`. Builds while Visual Studio has the solution open can show MSB3026 copy retries caused by file locks; the retries succeed.

## Test result

- The Browser and the Designer start and stay running.
- With AvalonDock 3.6.0 (and earlier 3.8.2), the Designer's UI Automation tree shows the menu, status bar, and the docked Tools, Pages, Outline and Properties panes with their title bars and auto-hide pins. The rendering itself was not seen: screen captures from this environment show WPF windows as blank white, for the 2.9 build as well.
- A script loaded a copy of `Data/WIREFLOW` through `FlatFolderRepository` with Newtonsoft.Json 13.0.4. The project and its `Main` page loaded, and saving produced byte-identical files.
- `PrivateExtractIcons`, which the shell uses, reads 16, 32, 48 and 256 px icons from both built exes. A desktop shortcut was not checked; the shell's icon cache can keep showing an old icon for a while.
- Off-screen `RenderTargetBitmap` renders of the real `MainDesignerWindow`, with and without a selected element, show the Properties pane dark throughout: list, category headers, editors, filter box.
- Off-screen renders of the real window in the Dark, Light and Classic themes, each switched to after the window was built (as the menu does), show the menus, panes, pane titles, Outline, Properties and the area around the page in the theme's colors. The Ctrl+Tab navigator window of AvalonDock keeps its white background; it was not checked.
- `.wf` files: a script ran 34 checks of `SqliteRepository` against copies of `Data/WIREFLOW` and `Data/Test`, all passing: the conversion keeps every file's JSON byte for byte and leaves the folder; a second open does not convert again; `SaveProject` archives and promotes as the folder storage does; masters and parts read their saved version; rename, delete, images; the Browser mode refuses `.project` files; other files are refused; the file is not held open. Off-screen renders show the real Designer converting and showing `Data/WIREFLOW`, and the Browser showing the resulting `.wf`.
- OptimaxTools.Dock: off-screen renders of the real Designer in Dark and Light show the docked panes, pane titles, the document tab, the Outline, the Properties pane and the page as with Xceed's AvalonDock; the pane buttons are upstream's newer icons. Docking, floating, auto-hiding and the Ctrl+Tab window were not exercised.
- Page frame: in off-screen renders of the real Designer, the frame lies just outside the page: `#5A5A5A` on the `#333333` around a `#333` page in Dark, `#B8BCC2` on `#CCCCCC` in Light; the real Browser in Dark shows it as well. Zooming and panning were not exercised.
- Splitters: in off-screen renders of the real Designer, all four splitters between panes carry the theme's style; pixels across them read gap, border, fill, border, gap: `#202020 #3A3A3A #282828 #3A3A3A #202020` in Dark and `#F3F4F6 #D9DCE1 #F3F4F5 #D9DCE1 #F3F4F6` in Light, crisp both vertically and horizontally. Dragging them and the auto-hide window's resizer were not exercised.
- `optimax-old.wf`: the migration script checked every page, layout and part (element counts against the 2016 files) and read each back through `SqliteRepository`; an off-screen render of the real Designer shows its home page with layout, parts, images and text.
- `Data` conversion: a script converted the five folder projects and checked each `.wf` against its folder, all passing: every document, draft, archived revision and image byte for byte, and every page, layout and part read back through `SqliteRepository` as through `FlatFolderRepository`.
- Off-screen renders of the real Browser window with a project open, in Dark, Light and Classic, show the menu, toolbar, page title and the area around the page in the theme's colors.
- Not tested: interactive use. That includes drawing a shape and watching the Outline update, docking, editing values in the property grid, opening a project in the Browser, and following links.

## Unresolved problems

- `packages/` is in `.gitignore`, but files in it are still tracked. The build does not use that folder.
- A part saved in the Designer cannot be read back (`GetPart` returns null), in both storages; see "Known traps" in `implementation.md`.
- The sample projects in `Data/` hold little: of the converted ones only `WireFlow.wf` has a page. `optimax-old.wf`, the migrated 2016 site, is the one with real content.

## Known regressions

None found.

## Recommended next step

Answer the open questions in `TO-DO.md`. Check the Designer and the Browser interactively, `.wf` files and OptimaxTools.Dock included (open a `.project`, edit, save, reopen; dock, float and auto-hide panes, Ctrl+Tab): View → Theme in each theme, docking, floating and auto-hiding panes, editing in the property grid, and following links in the Browser.
