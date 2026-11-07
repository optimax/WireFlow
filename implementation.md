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
| `OptimaxTools.Wpf.PropertyGrid` | The Designer's property grid, WireFlow's own: `OptimaxTools.Wpf.PropertyGrid` (see "Property grid"). XAML namespace `http://schemas.optimax.com/wpf/xaml/propertygrid` (prefix `pg`). |
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
- The page and what is drawn on it keep their own colors; the area around the page follows the theme (the palette's `PreviewBackground`). `DesignerPanel` draws a 1 px frame just outside the page in the palette's `PageFrame`, as PDFTools frames its pages, so that the page shows whatever its color; the frame follows zoom and pan. A new project's page background is `FlowProject.DEFAULT_BACKGROUND`, `#555`: a gray that differs from the area around the page in every theme (`#333333` in Dark, `#CCCCCC` in Light and Classic) without glaring in Dark. Both storages and Core's `Constants.DEFAULT_BACKGROUND` (the Browser's fallback for a color it cannot read) use it.
- Fluent's menus and toolbars are taller than the old ones: give the rows that hold them `Auto` height rather than a fixed one.
- Fluent pads list items by 12 px, 45 px rows. To make a list compact without losing the theme's item template, set the containers' `Padding` as a local value, as `MainDesignerWindow.CompactRows` does for the lists in `CompactLists`, Tools, Pages, Layouts, Parts, Images and History (27 px rows, as a Fluent menu item), as each list makes its rows and when the theme changes; Classic's own padding is already compact, so it is left there. An `ItemContainerStyle` would replace the theme's style, and one `BasedOn` it would keep the style of the theme current when it was loaded.

## Property grid

`OptimaxTools.Wpf.PropertyGrid` lists the browsable properties of `SelectedObject`'s type, as Xceed's grid did: by category (in the order of their names) or by name, with a filter box, a header naming the object's type and `Name`, and a name column whose width the grip between the columns sets. Rows are virtualized, grouped or not.

- `PropertyItem` is one property. Text goes through the property's `TypeConverter` in the invariant culture, as in XAML (`Auto`, `1,2,3,4`, `#FFFF0000`); empty text is null for a reference type other than string. A value that cannot be set leaves the property and sets `Error`, which the row shows as a red frame and a tooltip. While listed, each item watches its property (`AddValueChanged`), so a change made elsewhere shows; the watch ends when the selection changes.
- `PropertyValueChanged` is raised for a value set in the grid only, and only when it changed. Xceed's was raised for any change of a watched property, so it fired on every click; the Designer sets `IsModified` from it.
- `PropertyEditors` chooses each property's editor, as Delphi's `RegisterPropertyEditor` does: a registration names a property type (which also matches derived types), optionally narrowed to a type of object and a property name; the most specific matching registration wins, and the latest of equals. Without one: read-only text for a read-only property; a check box for a Boolean; a swatch with text for a brush or a color; a list of the system's fonts for a font family; a list for an enum, a font weight, style or stretch, or a converter's closed list of values; text otherwise. A `PropertyEditor` creates one control per row shown.
- An editor with a dialog (`HasDialog`, as Delphi's `paDialog`) gets a "…" button at the right end of the value, which, as Ctrl+Enter in the value, calls its `ShowDialog`; a value typed but not yet set is set first. The editor sets the value chosen with `PropertyItem.SetValue`, which raises `PropertyValueChanged`. A read-only property has no button.
- The Designer registers `ColorPropertyEditor` (in `App.Application_Startup`) for every `Brush`, `Color` and `Color?` property: the grid's swatch and text, and a dialog, `Views\ColorPickerDialog`, the color dialog of Optimax's other programs (MarkdownTools' MaxPad, PDFTools) on PixiEditor.ColorPicker. A brush property gets a solid brush; a gradient or image brush starts the dialog from white. MaxServices.Core has no color dialog, so the Designer has its own copy.
- The template gives `TextBox`, `CheckBox`, `ComboBox` and `ComboBoxItem` implicit styles of its own, inside the grid only: compact (22 px rows; Fluent's controls are 32 px tall) and in the grid's `Background` and `Foreground`, in every theme. Highlights and lines are the grid's `Foreground` at a low opacity, so they suit light and dark alike. The Designer sets `Background`, `Foreground` and `CategoryBackground` from its theme brushes.

## Elements

Every element a tool draws is a `BaseDesignerObject`, a `UserControl` with `Left`, `Top` and `Href` (Navigation) whose content is the WPF shape that draws it, built by its constructor: `WireFlowText` (kind `text`, a `WireFlowTextBlock`), `WireFlowRectangle` (`rect`, a `Rectangle`), `WireFlowEllipse` (`ellipse`, an `Ellipse`), `Box` (`box`) and `ImageBox` (`image`). Each forwards the properties of its content that make sense in Properties (`Fill`, `Stroke`, `StrokeThickness`, a text's `Text`, `TextAlignment`, `TextPadding` and others); a text's font, `Foreground` and `Background` are the element's own. The engine chooses the kind from the class's name in `ConvertUiElement` and the class from the kind in `RenderElement`: a new kind of element needs both. Because a `UserControl` reports the events of its content as its own (`e.Source`), a click anywhere on an element comes from the element, which the Navigate tool and selection rely on.

- The forwarded properties are plain properties, not dependency properties: a change made through Properties (its rows, its dialogs) is seen by the grid's other rows, but one made in code is not announced.
- `BaseDesignerObject` does not write its `Content` to XAML (`ShouldSerializeContent`), so that Duplicate, which copies through `XamlWriter` and `XamlReader`, gives the copy the content its constructor builds, which its properties change, rather than a second one.
- The saved `HorizontalAlignment` and `VerticalAlignment` of rectangles and ellipses are kept on the element, so that saving writes them again; a canvas ignores them.

## Text alignment

A text element (`WireFlowText`, holding a `WireFlowTextBlock`, a `TextBlock` as large as the element) places its lines across the frame with `TextAlignment` and down it with `TextVerticalAlignment`, saved as the element's `VerticalAlignment` (`Stretch`, which earlier texts have, is Top). `TextBlock` seals `MeasureOverride`, `ArrangeOverride`, `OnRender` and `OnPropertyChanged`, so the lines are moved down by the top padding: centered or at the bottom, it is the bottom padding plus the room a measuring `TextBlock` with the same font and wrapping finds left (half of it, or all). The text listens to the properties that change that room through metadata overrides in its static constructor, and to `SizeChanged`. A text shows its saved `Fill` as its background (`WireFlowEngine.TextBackground`); without one, or with one that is not a color, it is transparent, not black as `ColorUtils.StringToBrush` would make it. `WireFlowText` never has a null `Background`: none, as when it is cleared in the property grid, is coerced to Transparent, so that its whole frame can still be clicked. A text's padding is saved as its `StrokeThickness`.

In the property grid, `TextPositionPropertyEditor` is the editor of `TextAlignment` on texts and boxes. Its row shows only where the text lies, named as the chooser names the nine positions ("Top-Left", "Top", "Center", ...), and follows `TextAlignment`, `TextVerticalAlignment` and a box's `TextHorizontalAlignment` as they change, watching them (`PropertyDescriptor.AddValueChanged`) while the row is loaded. Its "…" button, Ctrl+Enter or a double click on the value opens `Views\TextPositionDialog`: MaxServices.Core's `PagePositionControl`, the 9-point chooser of PDFTools, in a "Text position" group with OK and Cancel, as PDFTools' Add Margins and Resize Pages dialogs show it. OK sets the alignments across and down (and a box's `TextHorizontalAlignment`) through `PropertyItem.SetValue` and `SetComponentProperty`, so each change raises `PropertyValueChanged`. A text's `TextVerticalAlignment` is listed in Appearance, as a box's is.

A box's `TextHorizontalAlignment` (where its text lies across it) carries its `TextAlignment` (where the lines lie) with it, but for `Stretch`, so that the two stay in sync when either row is used. The engine sets `TextAlignment` after it, so a saved box keeps the `TextAlignment` it was saved with.

## Stack panels

A stack panel (kind `stack`, `FlowStack` with a `Direction`, `Vertical` or `Horizontal`, and `Children`; on the page the `Stack` panel of Core) places its children one after another from its top left corner in its direction, as WPF's `StackPanel` does: each child keeps its size in that direction and is stretched across the panel in the other. `StackLayout.Arrange` calculates the places apart from any control. `Stack` writes each child's `Canvas.Left`, `Canvas.Top` and size across the panel when it arranges them, rather than only arranging them there, so the children keep true positions and sizes, which selection, saving, ungrouping and the Browser use as for any element; a change of direction therefore starts from the sizes the other direction gave. The panel clips its children to itself.

In the Designer, the Stack Panel tool draws one, in a faint translucent gray so that an empty panel shows; an element drawn starting inside a panel becomes its last child (the innermost panel, where they are nested), and is selected through it, as a group's elements are. Ungroup releases the children where they are.

## Tool glyphs

Each tool in the Designer's Tools list has a glyph before its title. The SVGs in `WireFlow.Designer\Glyphs` are their source: line glyphs on a 24 x 24 grid, as PDFTools' toolbar icons (stroke `currentColor`, 1.7 wide, round caps and joins, no fill), made of `path`, `line`, `polyline`, `polygon`, `rect`, `circle` and `ellipse` only. `Documents\make-tool-glyphs.cs` converts them once into `WireFlow.Designer\Xaml\ToolGlyphs.xaml` (`dotnet run Documents\make-tool-glyphs.cs` from the root), a `Geometry` per glyph keyed `ToolGlyph.<file name>` and the stroke width as `ToolGlyphStrokeThickness`, merged into `App.xaml`. The Designer uses the XAML only and reads no SVG: change an SVG, or add one, and run the script again rather than editing the XAML. It checks that WPF reads each glyph and that it stays inside the grid.

A tool names its glyph with `DrawingTool.Glyph` (the SVG's file name); `GlyphGeometry` finds it in the application's resources. The Tools list's template draws it 18 px square, in the row's own text color (`TextElement.Foreground`), so that it follows the theme and the selected row.

## Options

Tools → Options… is the Options dialog of MarkdownTools' and PDFTools' programs: `OptimaxTools.Options` 1.0.0, Optimax's own library made in Optimax.Shared.Lib, kept as a package in `LocalPackages\` as MaxServices.Core is. It has no look of its own, so it takes the theme's controls; `Views\DesignerOptionsDialog` derives from it, gives it the theme's colors, and adds the groups of options and their options. OK sets the dialog's results, which `MainDesignerWindow.OptionsMenuItem_Click` puts in `ConfigSettings` and saves; the dialog changes nothing itself. A new option is a setting in `ConfigSettings`, a control in the dialog's constructor, read in its `Apply`, and applied by the window.

- Files → Reopen the previous project at startup (`ConfigSettings.ReopenPreviousProject`, off by default): `SaveConfigSettings` keeps the project open as the settings are saved, as the Designer closes, in `PreviousProject` (null when none is open), with the tabs of the main document pane in `PreviousTabs` (kind and name, in order) and the one in front in `PreviousTab`. `Window_Loaded` opens it again once the window shows, if it is still there (`ReopenPreviousProject`), without its home page, and reopens the tabs whose page, layout or part it still has; with none, its home page.

The settings file is not offered for editing from the dialog (no Edit Settings File button): the Designer writes it as it closes, so edits made to it while the Designer runs would be lost.

## Publishing

`publish-locally.bat` publishes the WireFlow Designer, built in Release mode for `win-x64`, to `c:\Programs\Optimax\WireFlow`. The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64). The Browser is not published. The script empties the folder first (see "Known traps"), and stops if it cannot, for example while the published Designer is running.

## Third-party components and licensing

WireFlow itself is copyright Optimax Corporation.

| Component | Version | License | Used by |
|---|---|---|---|
| Newtonsoft.Json | 13.0.4 | MIT | Repository.Folder, Designer, Browser |
| PixiEditor.ColorPicker (with PixiEditor.ColorPicker.Models 1.0.11) | 3.4.2.3 | MIT | Designer (the color dialog) |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.31 | MIT | PixiEditor.ColorPicker |
| Dirkster.AvalonDock source (as OptimaxTools.Dock, in this solution) | 5.0.1 | MS-PL | Designer |
| Microsoft.Data.Sqlite (with Microsoft.Data.Sqlite.Core) | 10.0.14 | MIT | Repository.Sqlite |
| SQLitePCLRaw (bundle_e_sqlite3, core, lib.e_sqlite3, provider.e_sqlite3) | 2.1.13 | Apache-2.0 | Microsoft.Data.Sqlite |
| SQLite (`e_sqlite3.dll`, in SQLitePCLRaw.lib.e_sqlite3) | as built by SQLitePCLRaw 2.1.13 | Public domain | SQLitePCLRaw |

Extended.Wpf.Toolkit (Xceed's property grid) is no longer used. Do not bring it back: from 3.7.0 on, its versions are under the Xceed Community License (non-commercial use by fewer than 10 users), and 3.8.2's package declares MS-PL but holds the Community License.

Docking is OptimaxTools.Dock, from the source of Dirkster.AvalonDock 5.0.1 (MS-PL in its repository's `LICENSE` and its packages). Keep it close to upstream, so that later upstream fixes can be merged: change only what WireFlow needs, and mark each change with a `WireFlow:` comment.

`THIRD-PARTY-NOTICES.md` holds the license texts. Update it with the exact texts whenever a package is added, removed or upgraded.

## Project file format

A project is one SQLite database file, `<name>.wf`, written by `SqliteRepository`. `PRAGMA application_id` is `0x57464C57` ("WFLW") and `PRAGMA user_version` the format number, now 2; a file with another application id or another format number is not opened. The programs read format 2 only: the format 1 files, whose JSON named .NET classes, were upgraded once with `Documents\upgrade-wf-to-format-2.cs`, which can upgrade any other format 1 file left (`dotnet run upgrade-wf-to-format-2.cs -- <file.wf> ...`). The format has four tables:

- `Project` — one row: the `FlowProject` JSON (size, background, home page, `RevisionNo`).
- `Documents` — one row per page, layout and part (`Kind` `page`, `layout` or `part`, and `Name`): `Saved`, the JSON of the saved version, and `Draft`, the JSON of the unsaved edits. `SavePage`, `SaveLayout` and `SavePart` write the draft. `SaveProject`, in one transaction, raises `RevisionNo`, archives the project and the saved version of each document that has a draft under the new revision, and makes the drafts the saved versions.
- `Archive` — earlier versions (`Kind`, which can also be `project`, `Name`, `RevisionNo`, JSON): the version that the save of that revision replaced.
- `Images` — imported images (`Name`, the file name, and `Data`).

Names compare ignoring case, as file names did. Each document stays one JSON value rather than a table per element type: the element model is open-ended (an element's `Type` names its kind), and the programs read and write whole pages. As in the folder storage, a page is read with its draft, but a layout used as a master and a part are read in their saved version only. Renaming a layout or a part is not implemented, since pages and elements refer to them by name, and deleting or renaming a page or layout leaves its archived versions under the old name.

Each operation opens the file and closes it again: pooling is off, so the file is not held open between operations, and the Browser can view a project the Designer is editing. A new or converted file is built under a temporary name (`<name>.wf.tmp`) and then moved into place.

JSON is written and read by Newtonsoft.Json with `FlowJson.Settings` (in `WireFlow.Repository.Folder`, used through `FlatFolderRepository.SerializeToJSON` and `DeserializeFromJSON`), and names no .NET types:

- An element's `Type` is its kind, and chooses its class when it is read (`FlowElementConverter`): `group` is `FlowGroup`, `stack` (a stack panel) `FlowStack`, `part` (a part reference) `FlowPart`, `image` `FlowImage`, and `box`, `text`, `rect` and any other kind `FlowElement`. A new kind of element with a class of its own needs its kind added to `FlowElementConverter.Create`. A part document is read as `FlowPart`, a page or layout as `FlowPage`, the project as `FlowProject`.
- Enums are written by name; NaN, an automatic size, as `null`.
- Every value is written, defaults included, so reading never depends on the defaults in the code: changing `FlowElement.Defaults` or a constructor changes new elements only.
- `Type` and `Name` come first in each object.
- Unknown keys are dropped on load and are therefore lost on the next save.
- A page's or layout's `Width` and `Height` are its own size, or `null` when it takes the size of its nearest layout that has one, else the project's (`WireFlowEngine.InheritedPageSize`). The size of the page is set in the Designer as the canvas's `Width` and `Height` in Properties; `SerializePage` keeps it as the page's own when the page has one or it differs from the inherited size.
- A page's or layout's `Background` is likewise its own color, or `null` when it takes the background of its nearest layout that has one, else the project's (`WireFlowEngine.InheritedPageBackground`). It is set as the canvas's `Background` in Properties; `SerializePage` keeps it as the page's own when the page has one or it differs from the inherited background, and a background cleared in Properties makes the page follow its layout or the project again. `RenderPage` gives the canvas the page's background, so the Designer and the Browser show it alike.

**Folder projects.** The earlier storage, `FlatFolderRepository`, keeps a project as a folder: `<name>.project` (the `FlowProject` JSON); `pages/*.page`, `layouts/*.master` and `parts/*.part`, with unsaved edits in `*.new` files beside them; `pages.archive/` and the other `*.archive/` folders, holding `<name>.<revision>` files; and `images/`. When the Designer opens a `.project`, `SqliteRepository` converts it into `<name>.wf` beside it, writing every document again in the current JSON, and opens that; the folder is left as it is. If that `.wf` file already exists, it is opened, not replaced. The Browser opens `.wf` files only and says so when given a `.project`. The sample projects in `Data` are `.wf` files only. `optimax-old.wf` is a 2016 project migrated once from an earlier format still (`Optimax.wireflow`, elements named only by `Type`, without `$type` tags), which no storage reads.

## Known traps

- **Inside the `OptimaxTools.*` namespaces, `Dock` is the namespace `OptimaxTools.Dock`, not WPF's `Dock` enum.** C# finds the namespace, a member of `OptimaxTools`, before the `using System.Windows.Controls` of the file, so `Dock.Top` does not compile there, and a file-level `using` alias does not help. Write `System.Windows.Controls.Dock.Top`, as the fork does, when merging upstream code.
- **Scratch programs that host the Designer's window.** Creating `App` queues its `Startup` handler, which opens a real Designer window as soon as the dispatcher runs and applies the saved theme. `MainDesignerWindow` also writes its settings (`%APPDATA%\WireFlow\WireFlow.Designer.config`) when it closes. Remove the `Startup` handler, and keep a copy of the settings file to put back.
- **Single-file `dotnet run app.cs` scripts default to `PublishAot=true`,** which disables dynamic code generation, unlike the apps. Add `#:property PublishAot=false` to scripts that load WireFlow documents, so that Newtonsoft.Json runs as it does in the apps.
- **`dotnet publish` into an existing folder copies a file only when it is newer than the one there.** After a package downgrade, the publish succeeds but keeps the newer DLLs of the old version. Publish into an empty folder, as `publish-locally.bat` does.
- **Visual Studio's design-time builds share `obj/` with command-line builds.** A command-line build while the solution is open in Visual Studio can fail with CS2001 (a missing `*.g.cs`); building again succeeds.
- **Line endings.** The repository has `core.autocrlf=true` and no `.gitattributes`: the index holds LF, the working tree CRLF. Git Bash `sed -i` rewrites a CRLF file as LF; run `unix2dos` on it afterwards.
- **Microsoft.Data.Sqlite 9.x brings SQLitePCLRaw 2.1.10,** whose `lib.e_sqlite3` NuGet flags with a high-severity advisory (NU1903, GHSA-2m69-gcr7-jv3q). Microsoft.Data.Sqlite 10.0.14 brings 2.1.13, which is not flagged. Keep the NuGet audit warnings visible when upgrading it.
- **`DesignerPanel` lives in Core, but the `DesignerBackgroundBrush` and `PageFrameBrush` resources exist only in the Designer.** It refers to them with `SetResourceReference`, which leaves them unset where they are missing, so the Browser can host it; the Browser sets `Background` and `PageFrameBrush` from the palette itself.
- **The Designer's Outline view is a snapshot.** `MainDesignerWindow.PopulateOutlineTree` rebuilds it on navigation, on a tab switch, and when the current `DesignerNavigator` raises `ElementsChanged`. A new `DesignerNavigator` operation that adds, removes or reorders elements must call `OnElementsChanged()`, or the Outline goes stale. Its entries' `IsHighlighted` follows the current designer's `SelectedElements`: `UpdateOutlineHighlight` runs on `SelectionChanged` and after each rebuild, so a change of selection must raise `SelectionChanged` even when `CurrentElement` stays, as `DeselectElement` does.
- **A property grid editor gets the grid's implicit styles.** A `PropertyEditor` that creates a `TextBox`, `CheckBox` or `ComboBox` gets the grid's compact style, not the theme's; give a control a `Style` of its own to look otherwise.
- **A WPF `ListBox` reports a cleared selection as `SelectedIndex` -1.** Code that indexes `Items` with it must check the range.
- **.NET Framework's `System.Drawing.Icon` skips PNG-compressed icon images,** such as the 256 px images of the application icons, so Windows PowerShell cannot load them that way. Check a built exe with the shell's `PrivateExtractIcons` instead.
- **WPF properties in Properties can break the Designer.** The grid lists every browsable property of the WPF element selected. `Panel.IsItemsHost`, set on a page panel (`BaseElement`'s subclasses, `DesignerCanvas`), makes WPF refuse access to its children, and the next click crashed. Such a property is hidden by a `new` property marked `[Browsable(false)]` that forwards to the base one.
- **An element without a `Type` is read as a `FlowElement`,** and shown as an unknown object: the kind lives only in `Type`.
