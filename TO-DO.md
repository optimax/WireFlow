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

## [DONE] Reopen Previous

"Reopen previous project on startup" option should ensure that not only the project re-opens, but all the tabs within that project that were open get re-opened.

Done. With the option on, the Designer reopens the project with the tabs that were open when you closed it: pages, layouts and parts, in the same order, with the same tab in front.

- A tab whose page, layout or part has since been deleted or renamed is skipped.
- If none is left (or you had closed them all), the home page opens, as before.
- Tabs dragged out into a floating window are not remembered.

## [DONE] Text Alignment Custom Editor

Text Alignment Custom Editor should not be embedded into the property grid directly. It should be invoked via `...` button and shown in a popup like any other property. Only the value should be shown in the property grid directly.

Done. The TextAlignment row of a text or a box now shows only the value: where the text lies in its frame, named as the 9-point chooser names it (Top-Left, Top, Top-Right, Left, Center, Right, Bottom-Left, Bottom, Bottom-Right). Its `...` button opens the chooser in a small dialog, as Background opens the color dialog; Ctrl+Enter or a double click on the value open it too. The dialog is laid out as PDFTools shows the chooser in Add Margins and Resize Pages: the chooser in a "Text position" group, its label at the right, then OK (Enter) and Cancel (Esc). OK moves the text; Cancel leaves it.

- Justified text shows as, for example, "Top-Left (justified)", and stays justified unless you choose another position.

## [DONE] TextAlignment vs TextHorizonalAlignment, TextVerticalAlignment

TextAlignment needs to be in sync with TextHorizonalAlignment and TextVerticalAlignment. When changing one of TextHorizonalAlignment, or TextVerticalAlignment, TextAlignment's highlight remains where it was. In custom editor shown only on demand, it should not be a problem, but the value should be in sync with the other two.

Done. The TextAlignment row follows the other two as they change: set TextVerticalAlignment to Bottom, and it reads Bottom-Left at once; the dialog always opens at the current position.

- On a box, changing TextHorizontalAlignment now moves the lines with the text (TextAlignment follows: Left, Center or Right), so the two no longer disagree. Stretch leaves TextAlignment as it is.
- A text (not a box) now lists its TextVerticalAlignment under Appearance too, as a box does; before, only the chooser could set it.
- Saved boxes keep their own TextAlignment when loaded, so existing pages look as they did.

## [DONE] All Tools Derive from BaseDesignerObject

Make all element tools, including Text, Rectangle, Ellipse, derive from BaseDesignerObject, exposing  (forwarding) the properties that make sense, so that all can support HRef property, in preparation for the next task.

Done. Text, Rectangle and Ellipse are now BaseDesignerObjects, as Box and Image already were: each is a small control holding its WPF shape, and Properties shows the shape's properties that make sense (Fill, Stroke, StrokeThickness, RadiusX/RadiusY for a rectangle; Text, TextAlignment, TextVerticalAlignment, TextWrapping, TextPadding, LineHeight, Background, Foreground and the font for a text) and Href, under Navigation.

Found and fixed on the way:

- Ellipses were saved but never drawn again: a page with an ellipse showed a pink "unknown object" in its place after reload, and saving that page again turned it into a rectangle. Ellipses now load as ellipses.
- Duplicate copies an element through XAML, which would also write its inner shape; it no longer does, so each copy builds its own shape, and changing the copy's properties changes what it shows (checked for texts, rectangles, ellipses and boxes).
- A text's padding and a rectangle's saved alignments were lost on save; they are kept now.

## [DONE] Href on All Elements

Ensure that a hyperlink target (Href) can be defined on any element (at least any that has a visual extent): Text, Rectangle, etc., not only on Image or Box.

Done. Every element with an extent now has Href, under Properties → Navigation: Text, Rectangle, Ellipse, Box and Image (from the task above), and also groups, stack panels and part references. Layers have none.

- A click follows the innermost link around what you clicked: an element's own link first, else that of the group, stack panel or part it is in. So you can link a whole group (say, a card) and still give one element inside it a different link.
- The Browser and the Designer's Navigate tool behave alike.
- Links are saved with the page; existing files need no conversion.

## [>>WORKING<<] Convert to a DOM (Both)

This applies to both the Designer and Browser. 

Currently, the WPF element tree itself is the Designer’s working model. The Flow* data model only exists while a page is being loaded or saved. Redesign the flow so that the DOM becomes the source of truth and the ultimate authority about the model, independent of its presentation. Treat WPF as a presentation layer only, one of possibly many. The saved data is another "presentation". Make sure that WPF-specific details do not spill into other layers or services.

Plan, in phases; each one is committed on its own, with the Designer and the Browser working after it:

1. A WPF-free DOM. The Flow* classes (`WireFlow.Types`) become the DOM: they lose every WPF type (alignments, text wrapping, font stretch, image stretch, stack direction become WireFlow's own values with the same names, so `.wf` files stay exactly as they are), and the project no longer uses WPF.
2. Observable DOM. Nodes announce changes to their properties and children, and know their parent.
3. WPF as a view of the DOM. The Designer keeps each open page, layout and part as its DOM; the WPF elements are drawn from it and follow its changes; saving writes the DOM, not the WPF tree.
4. Edits go through the DOM. Properties shows the selected element's node; drawing, moving, resizing, deleting, duplicating, grouping and ordering change nodes, and the view follows. This is also what Undo/Redo (the next task) will record.
5. The Browser draws the DOM through the same view, read-only.

Progress: phases 1 and 2 are done. The Flow* classes use no WPF type any more (the project no longer uses WPF); they tell of every change of their properties and children, and each node knows its parent. The files are unchanged: all 30 documents of the 7 sample projects read and write back the same JSON. Nothing changes in the Designer yet: phase 3 is next.

Found while testing: drawn and saved again through WPF, as the Designer saves today, a page loses detail: a color name such as White comes back as #FFFFFFFF, a rectangle without a stroke gets a black one, and a group's automatic size becomes fixed numbers. Phase 3, in which saving writes the DOM and not the WPF tree, ends that.

**Q1: I am making the existing Flow* classes the DOM (observable and WPF-free), rather than adding a separate set of DOM classes beside them, and keeping the `.wf` format unchanged. OK?**

**Q2: After phase 4, Properties shows an element's own properties (Fill, Stroke, Text, Href, position and size, font, alignment and so on), no longer every WPF property of the control that draws it (Margin, Cursor, IsHitTestVisible, Focusable and others, which are not saved anyway). OK?**

**Q3: Colors stay text in the DOM (`#AARRGGBB` or a color name), as the files keep them; the color dialog keeps working on them.**

## Support Undo/Redo

Redesign the app, so that unlimited Undo/Redo is supported. Undo history must be kept since the last save.

## Avoid Rebuilding Entire Outline

Avoid rebuilding the entire outline on every change. Rebuild/update only the elements of the outline that are affected by the change. Make outline virtualizing, so that when the tree gets long and complicated, it renders fast.




## END-OF-TO-DO

