---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## BUG: Renaming Page

Attempting to rename a page in the Pages list seems to work in some places, but in that list, the name of the page does not change in the list. Moreover, the page remains selected and there is no way to deselect it any more. The image below shows Main page having been renamed to HOME - but it does not show, and now two pages are selected in the list.

![pasted-12](<TO-DO.images/rename-page-bug.png>)

Additionally, links that targeted the old name (Main) are now broken throughout the model.

## Esc Should Work Anywhere

Pressing Esc should deselect the current tool and return to Select mode regardless of which pane has focus, not only in the middle Designer pane. This includes Navigate mode/tool.

## BUG: Save As

Save As does not work.

## BUG: Options | Navigate Mode

![pasted-12](<TO-DO.images/pasted-12.png>)

The color selections for Navigate Mode in Options should be regular color pickers with swatches, not a dropdown with names.

## BUG: Pane Buttons

![pasted-13](<TO-DO.images/invisible-caption-buttons-bug.png>)

The buttons that should appear in the captions of side panels are invisible in dark themes.

## Navigate Mode Color Definitions

The ones that are define in Tools|Options (global app settings) are the defaults, but the options should be saved as part of the document itself, since the colors depend on the colors in the model, to be visible and sufficiently contrasting.  

If a model is open, Tools|Options adjust the model's colors.

If Tools|Options is open without a document, whatever is set there defines the defaults for new documents/models.

## Save All Toolbar Button

Add "Save All" button to the Toolbar.

## Align: Fill Space

Add to the available Alignment options: Fill Space, that makes the selected control(s) fill the available vertical and horizontal space.

## Cancel Navigate Mode

![pasted-11](<TO-DO.images/pasted-11.png>)

Add an "x" glyph to navigate mode indicator to cancel navigate mode and return to Select tool.

## Renaming Pages Continued

Undoing a rename made in Properties gives the page its old name back in the page itself, but the Pages list keeps the new name until the page is saved. Sync all changes so that the display is consistent in real time.

## Font Dialog: Foreground Color

Add Foreground Color selector to the Font dialog. Make the dialog wider than it is tall (dialogs should always strive to present in landscape rather than portrait orientation).

## Font Dialog Lists

The Font dialog's lists (Font, Style, Size) have their items way too far apart. Make the lines 100% closer (eliminate almost all the line spacing). This applies in general in this project: whenever you display a list, it needs to have its items tightly spaced.

## Panel Container Element

Create a regular panel container, in which children (any other element) can be aligned based on their Alignment property (add Alignment to the base object): None, Top, Bottom, Left, Right, Fill. The Alignment property will be settable either by a dropdown listing the values (enumeration) or by a custom editor that pops up with the 9-buttons. 

## Setting Layout

It should be possible to choose a Layout from a dropdown in Properties, in addition to being able to type its name or ID.

Done. A page's or layout's Layout row in Properties is now a drop-down list of the project's layouts by name, with an empty entry for none. Its box still takes a layout's name or ID typed in, which is kept as typed. A layout chosen is set as the list closes; one typed is set with Enter or when the box loses the focus, and Esc takes the typing back. A name or ID of no layout is refused, and so is a circle: a layout set on itself, or on a layout that is set on it, nearer or farther. The row shows the reason. The page in front shows the layout chosen at once, drawn under its own layers, and loses it when the row is cleared. Checked by a script on a copy of Data/WireFlow.wf (12 checks); not by hand.

TODO: Empty items should not be shown in the dropdown. It should be a dropdown box (typing allowed), not a dropdown list (select only). I specifically said "being able to type".

## Fit Height, Fit Width

Sizing submenu offers Make Same Height and Make Same Width. Add Fit Vertically and Fit Horizontally:

- Fill Height: Places the element such that it completely fills the *available vertical space* in its container so, if invoked between a header and a footer rectangle, it would fill the remaining space between the header and footer.
- Fill Width:  Places the element such that it completely fills the *available horizontal space* in its container.

## Snap to Other Elements

When moving or resizing an element (with the mouse), make it snap to the edges of other elements, including the page edge.

## Stack Panel "Stacking-From"

Add a "Stacking-From" property to the StackPanel, with values `Beginning`, and `Ending`.

`Beginning` Stacking behaves like today. `Ending` stacking starts the stacking from the opposite end of the panel, so a vertical stack panel with `Ending` stacking would be stacking from the bottom.

## Show History

Fill the History list with the saved versions.

## Duplicate Behavior

When adding a page with the same name as an already existing one, the dialog is dismissed first and disappears, and only then the error message pops up. Pop up the duplicate error message before dismissing the dialog (on validation), and let the dialog remain for the user to correct the duplicate name.

## END-OF-TO-DO

