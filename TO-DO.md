---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Support Undo/Redo

Redesign the app, so that unlimited Undo/Redo is supported. Undo history must be kept since the last save.

Done. Edit → Undo (Ctrl+Z) and Redo (Ctrl+Y or Ctrl+Shift+Z), without limit, for the page or layout in front, back to when it was opened or last saved.

- The history records the DOM itself: every change of every node of the page, and every element added, removed or moved. So every edit can be undone, whatever made it: drawing, moving, resizing, aligning, sizing, duplicating, deleting, grouping, ungrouping, ordering, and values set in Properties.
- A drag of the mouse or a resize is one step, however far it goes; any other edit is one step, even when it moves many elements (aligning ten elements is undone at once). Each arrow key press is a step.
- Each open tab has its own history. Saving clears it, as does opening another page in the tab. Undoing every edit since the save makes the page unmodified again.
- In a text box (in Properties, say), Ctrl+Z undoes the typing in the box, as usual.
- Tested with a script (27 checks), not by hand in the running Designer: please try Undo and Redo after the edits above.

## [DONE] BUG: TextAlignment vs TextHorizonalAlignment, TextVerticalAlignment

TextAlignment needs to be in sync with TextHorizonalAlignment and TextVerticalAlignment. When changing one of TextHorizonalAlignment, or TextVerticalAlignment, TextAlignment's highlight remains where it was. In custom editor shown only on demand, it should not be a problem, but the value should be in sync with the other two.

Done. The TextAlignment row follows the other two as they change: set TextVerticalAlignment to Bottom, and it reads Bottom-Left at once; the dialog always opens at the current position.

- On a box, changing TextHorizontalAlignment now moves the lines with the text (TextAlignment follows: Left, Center or Right), so the two no longer disagree. Stretch leaves TextAlignment as it is.
- A text (not a box) now lists its TextVerticalAlignment under Appearance too, as a box does; before, only the chooser could set it.
- Saved boxes keep their own TextAlignment when loaded, so existing pages look as they did.

BUG: TextHorizontalAlignment is no longer even shown.

Fixed. A box listed it (under Text, at the end), but a text did not. Both now do, and on both, setting it moves the lines with it (TextAlignment follows: Left, Center or Right), and the TextAlignment row and its chooser follow it; the chooser sets it too.

## BUG: Properties of Multiple Objects

When multiple objects are selected, the Properties grid should show the properties that all of the selected objects have in common, not the last selected object's properties. Changing a value of such property should apply to all the selected objects.

## BUG: Image Crashes App

Placing an image object on a page crashes the app.

## BUG: Color Selection Dialog Controls

Color Selection Dialog (Fill, Background, Stroke, etc.) overlaps color component textboxes. Also, it does not provide enough room for the "Web color" box, especially, when the focus is on it and the clear-X appears.

![pasted-4](<TO-DO.images/pasted-4.png>)

## Color Selection Dialog

Color Selection Dialog needs a number of predefined swatches near the bottom: Black, White, Light Gray (#dddddd), and Gray (#999999). Additionally, it needs to remember and show swatches for the last 10 colors selected. Clicking such a swatch would set the dialog's color to what the swatch represents. 

![pasted-5](<TO-DO.images/pasted-5.png>)

Remove the above-highlighted control in the middle.

## Convert to a DOM (Both)

This applies to both the Designer and Browser. 

Currently, the WPF element tree itself is the Designer’s working model. The Flow* data model only exists while a page is being loaded or saved. Redesign the flow so that the DOM becomes the source of truth and the ultimate authority about the model, independent of its presentation. Treat WPF as a presentation layer only, one of possibly many. The saved data is another "presentation". Make sure that WPF-specific details do not spill into other layers or services.

Plan, in phases; each one is committed on its own, with the Designer and the Browser working after it:

1. A WPF-free DOM. The Flow* classes (`WireFlow.Types`) become the DOM: they lose every WPF type (alignments, text wrapping, font stretch, image stretch, stack direction become WireFlow's own values with the same names, so `.wf` files stay exactly as they are), and the project no longer uses WPF.
2. Observable DOM. Nodes announce changes to their properties and children, and know their parent.
3. WPF as a view of the DOM. The Designer keeps each open page, layout and part as its DOM; the WPF elements are drawn from it and follow its changes; saving writes the DOM, not the WPF tree.
4. Edits go through the DOM. Properties shows the selected element's node; drawing, moving, resizing, deleting, duplicating, grouping and ordering change nodes, and the view follows. This is also what Undo/Redo (the next task) will record.
5. The Browser draws the DOM through the same view, read-only.

Done, all five phases, under the assumptions of the questions below (still open, hence [QUESTION]). Phases 3 to 5 were done together rather than one by one: with saving writing the DOM, an edit still made on the WPF elements would have been lost, so the view following the DOM and the Designer editing it had to arrive at once.

- The DOM is the model. The Designer keeps each open page or layout as its DOM, and saves the DOM; the Browser draws the DOM it reads. The WPF elements are a view: each is tied to its node and follows it, in place, so the selection stays on it; adding, removing or reordering a layer's or group's nodes adds, removes or reorders their elements.
- Every edit changes nodes: drawing, moving (mouse and arrow keys), resizing, aligning, distributing, stacking, sizing, duplicating, deleting, grouping, ungrouping and ordering. Undo/Redo can record them at the DOM.
- Properties shows the selected element's own properties for its kind (a rectangle no longer lists fonts, margins or cursors), under the names it had (TextPadding, BorderColor, TextVerticalAlignment, ...), and the page's size and background when nothing is selected. Colors keep the swatch and the color dialog; the text position keeps its chooser.
- Saving no longer loses detail: color names stay names, a rectangle without a stroke keeps none, a group's size is not fixed. All 8 pages of the sample project, drawn and saved unchanged, write the same JSON.
- Tested with scripts (view and Properties: 26 checks; the Designer's operations: 18 checks), not by hand in the running Designer: please try drawing, moving, resizing, grouping and Properties.

**Q1: I am making the existing Flow* classes the DOM (observable and WPF-free), rather than adding a separate set of DOM classes beside them, and keeping the `.wf` format unchanged. OK?**

A1: Yes.

**Q2: After phase 4, Properties shows an element's own properties (Fill, Stroke, Text, Href, position and size, font, alignment and so on), no longer every WPF property of the control that draws it (Margin, Cursor, IsHitTestVisible, Focusable and others, which are not saved anyway). OK?**

A2: Yes. We may have to adjust and show specific additional properties later on.

**Q3: Colors stay text in the DOM (`#AARRGGBB` or a color name), as the files keep them; the color dialog keeps working on them.**

A3: Yes.

## Stack Panel Behavior

![pasted](<TO-DO.images/pasted.png>)

Place a rectangle inside a vertical StackPanel. --> The rectangle does not span the panel horizontally as it should but seems clipped on the right instead.

## Stack Panel Content in Outline

A Rectangle inside a StackPanel does not show up in the Outline. Any objects placed inside a StackPanel should be listed and be selectable in the Outline.

## Stack Panel HitTest

An object inside StackPanel cannot be selected by clicking on it. The entire panel is being selected on click, instead of individual objects.

Expected: When an object inside the StackPanel is clicked on, that object becomes selected. Pressing Esc should unselect object but select it's parent/owner container. Eventually, the Page  itself becomes selected.

## Avoid Rebuilding Entire Outline

Avoid rebuilding the entire outline on every change. Rebuild/update only the elements of the outline that are affected by the change. Make outline virtualizing, so that when the tree gets long and complicated, it renders fast.

## [DONE] TextHorizontalAlignment

Is missing from Text and Box elements.

Done with the bug above: texts and boxes both list TextHorizontalAlignment under Text, in sync with TextAlignment.

## NodeProperties

![pasted-3](<TO-DO.images/pasted-3.png>)

The Properties grid should identify the type of the currently selected node, instead of saying "NodeProperties" for everything.,

## ID Property

In addition to Name, every element should define a common property ID. The ID should be the target/content of an Href link.

## Property Grid and Arrow Keys

Up and Down arrows should navigate up and down in the property grid, like in a spreadsheet.

## Outline Caption and Menu

![pasted-6](<TO-DO.images/pasted-6.png>)

Add an Add Layer and Delete small buttons in the Caption of the Outline, to make it possible to insert new Layers and to delete the selected objects. Add corresponding commands to the Outline menu.

## Remember Window Position and Panes

Make sure that the window position, layout (the open panes), and the splitter positions are remembered from run to run.

## View Menu Toggles

For each pane in the application (Outline, History, Tools, Pages, Layouts, etc.), make it possible to toggle its visibility via View menu. 

## Pages: An Ordered List

Pages should be an ordered list, with move up and move down buttons in the caption bar, as well as drag-and-drop supported. The first page in the list is the default entry point for the model.

## Font Family in Properties

Font Family should have a custom editor that brings up a font chooser and sets multiple font-related properties, not just the FontFamily.


## END-OF-TO-DO

