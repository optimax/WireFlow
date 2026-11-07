---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] BUG: Background Not Saved

User-defined page background does not survive project save/reload.

Fixed. A page had no background of its own in the file: every page was drawn in the project's background, so the one you gave the page in Properties was never saved. Now a page, or a layout, keeps the Background you give it (the canvas in Properties, with nothing selected), and it shows again after save and reload, in the Designer and in the Browser.

- A page whose background you leave alone stores none, and keeps following its layout's background, or else the project's. A background given to a layout reaches the pages that use it.
- Clearing a page's Background in Properties makes it follow its layout or the project again.
- Existing files need no conversion: their pages simply have no background of their own yet.

## [DONE] Pages Line Spacing

Pages list's lines are too far apart. Make them the same spacing as Tools palette. The same applies to Layouts, Parts, Images, and History.

Done. Pages, Layouts, Parts, Images and History now have the Tools palette's row spacing: 27 px rows in Dark and Light, as a menu item, instead of the theme's 45 px, and Classic's own compact 23 px rows. All six lists share one piece of code, so they stay alike, also after a change of theme and as new rows appear.

History is still empty: nothing fills it yet. Its rows will be as compact when something does.

## [DONE] BUG: StackPanel Crashes App

Place a StackPanel on a page. Check its `IsItemsHost` property. Click on the panel — the app crashes.

Fixed. `IsItemsHost` is a WPF panel property that hands the panel's children to a list control (an `ItemsControl`). A page element is in none, so once it was checked WPF refused every access to the panel's children ("A panel with IsItemsHost="true" is not nested in an ItemsControl"), and a click, which reads them, crashed the Designer. It means nothing for a wireframe, so Properties no longer lists it: not for stack panels, nor for groups, layers, parts and the page itself, which are panels too and crashed the same way.

## [DONE] Ctrl+S

Make it a shortcut for File|Save.

Done. Ctrl+S is File → Save, and the menu shows it. As the menu does, it saves every modified page, layout and part, and the project; a project not saved yet asks for a file name first.

## [DONE] Esc Cancel Specialized Tool

Pressing Esc should revert the tools selection to Select. 

Done. Esc chooses the Select tool again, from any other tool, wherever the keyboard is in the window: the page, the Tools list, Properties. On the page it also still does what it did: it ends a drag and clears the selection.

## Reopen Previous

"Reopen previous project on startup" option should ensure that not only the project re-opens, but all the tabs within that project that were open get re-opened.

## Text Alignment Custom Editor

Text Alignment Custom Editor should not be embedded into the property grid directly. It should be invoked via `...` button and shown in a popup like any other property. Only the value should be shown in the property grid directly.

## TextAlignment vs TextHorizonalAlignment, TextVerticalAlignment

TextAlignment needs to be in sync with TextHorizonalAlignment and TextVerticalAlignment. When changing one of TextHorizonalAlignment, or TextVerticalAlignment, TextAlignment's highlight remains where it was. In custom editor shown only on demand, it should not be a problem, but the value should be in sync with the other two.

## All Tools Derive from BaseDesignerObject

Make all element tools, including Text, Rectangle, Ellipse, derive from BaseDesignerObject, exposing  (forwarding) the properties that make sense, so that all can support HRef property, in preparation for the next task.

## Href on All Elements

Ensure that a hyperlink target (Href) can be defined on any element (at least any that has a visual extent): Text, Rectangle, etc., not only on Image or Box.

## Convert to a DOM (Both)

This applies to both the Designer and Browser. 

Currently, the WPF element tree itself is the Designer’s working model. The Flow* data model only exists while a page is being loaded or saved. Redesign the flow so that the DOM becomes the source of truth and the ultimate authority about the model, independent of its presentation. Treat WPF as a presentation layer only, one of possibly many. The saved data is another "presentation". Make sure that WPF-specific details do not spill into other layers or services.

## Support Undo/Redo

Redesign the app, so that unlimited Undo/Redo is supported. Undo history must be kept since the last save.

## Avoid Rebuilding Entire Outline

Avoid rebuilding the entire outline on every change. Rebuild/update only the elements of the outline that are affected by the change. Make outline virtualizing, so that when the tree gets long and complicated, it renders fast.




## END-OF-TO-DO

