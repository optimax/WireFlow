---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] BUG: Adding Images to Project

![pasted-8](<TO-DO.images/pasted-8.png>)

There is no way to add an image to the project. Also, OK and Cancel buttons are clipped rendering their text almost invisible.

Fixed. The dialog's + and − did nothing: they were copied from the Images pane without the commands behind them. Now + imports image files into the project (several at once) and selects the first, and − deletes the selected image after asking; the Images pane shows the change. OK and Cancel are full size (Enter is OK, Esc Cancel). Checked by a script that opened the dialog with the Designer's theme (both buttons enabled as they should be, OK and Cancel whole); the file dialog and the delete were not tried by hand.

## [DONE] BUG: Color Selection Dialog Controls

Color Selection Dialog (Fill, Background, Stroke, etc.) overlaps color component textboxes. Also, it does not provide enough room for the "Web color" box, especially, when the focus is on it and the clear-X appears.

![pasted-4](<TO-DO.images/pasted-4.png>){width="200" style="max-width: 80%"}

Fixed, with the item below. The H, S, V and A values (and R, G, B, L) show in full: the theme's text boxes were wider than the room the sliders give them. The Web color box is wider, so #AARRGGBB shows whole with the clear-X. Checked by drawing the dialog to an image with the light theme; not by hand.

BUG STILL THERE: 

![pasted-7](<TO-DO.images/pasted-7.png>)

The text boxes are not high-enough.

Fixed. The color picker fixes those boxes at 18 pixels high, and the theme's text box keeps 4 of them for its border and inner margin, which left 14 for text 16 high. The boxes now take the height their text needs (20), on every tab. Checked by a script that measured them and drew the dialog in the light theme; not by hand.

## [DONE] Stack Panel Behavior

![pasted](<TO-DO.images/pasted.png>)

Place a rectangle inside a vertical StackPanel. --> The rectangle does not span the panel horizontally as it should but seems clipped on the right instead.

Fixed by the conversion to a DOM: a rectangle drawn inside a vertical stack panel now spans it (checked by a script: an 800 px wide panel, a rectangle drawn 430 px wide inside it shows 800 px wide at its left edge, and stays so after its node changes). The rectangle keeps its own width in the file, so it comes back if it is taken out of the panel.

TODO: This is not working. The last rectangle in WireFlow > Page 2 still does not span the width of the stack panel. Apparently, only the last object in StackPanel does not get properly stretched.

Fixed. The rectangles did span the panel, but each still showed only as much of itself as its own width: when the panel widened a child, the child was first laid out in a slot of its old width, and WPF kept it clipped to that slot. On Page 2 that hid the right part of the first and the last rectangle (the second, 745 of 796, showed nearly whole). The panel now places each child at its new size itself. Checked by scripts on a copy of WireFlow.wf, Page 2, drawn as the Browser and as the Designer draw it, also after editing, drawing another rectangle into the panel, undo and redo; not by hand.

## [DONE] New Layout Dialog Look

The New Layout dialog does not give its controls sufficient height, making the text clipped.

![pasted-9](<TO-DO.images/pasted-9.png>)

Fixed. The dialog had a fixed height and rows and buttons sized for the old theme; it now takes the height its fields need, with OK and Cancel at the right, full size. New Layout and New Page are the same dialog, and New Part, which had the same fault, is fixed too. Checked by a script that opened them with the theme (every field and button whole) and drew New Layout; not by hand.

## [DONE] ID Property

In addition to Name, every element should define a common property ID. The ID should be the target/content of an Href link.

Done, under the assumptions below. Every element, and every page and layout, has an ID in Properties (under Common, after Name), empty until you set one; it is saved with the project. A link (Href) can name a page, as before, or an ID: the ID of a page leads to that page, and the ID of an element leads to the page that holds it. This works in the Browser and with the Designer's Navigate tool. Names and IDs compare ignoring case, as page names do. With several objects selected, Properties does not offer ID (each keeps its own), as it does not offer Name. Checked by a script against a copy of a sample project; not by hand.

Q1: Should a new element get an ID by itself (say `rect-12`, unique in the project), or stay without one until you give it one, as now?

A1: Give it an automatic one.

Q2: Should the Designer refuse an ID that another element or page already has? Now nothing checks, and a link goes to the first page found that has it.

A2: Refuse, if it is not too expensive to check. Otherwise the first found link.

Q3: When a link names an element's ID, the Browser opens the page holding it. Should it also show which element (scroll it into view, mark it briefly)?

A3: No. Just open the page.

Q4: Should links keep naming pages by name, or should they name IDs only? Both work now; a page's name is tried first.

A4: IDs only. Change the Hrefs of existing projects accordingly.

Done, as answered. A new element gets an ID by itself, its kind and a number (rect-1, ellipse-2, ...), unique in the project; so does a copy, and a new page gets one made of its name ("Contact Us" gives contact-us). Properties refuses an ID that another element or page already has, and says so. A link leads to the page with that ID, or to the page holding the element with that ID; a page's name alone no longer works. The projects in Data were converted: each page got an ID made of its name (Page 2 is page-2), and each link that named a page by its name now names that page's ID; WireFlow.wf and FirstTest.wf, yours, are converted but not committed (copies from before are kept). New layouts and parts get no ID by themselves, and layers none, as links lead to pages. Checked by a script on a converted copy of WireFlow.wf (14 checks); not by hand. If the Designer had WireFlow.wf open while it was converted, a page it saves afterwards writes back its old links: close it without saving, or convert it again with Documents\upgrade-links-to-ids.cs.

## [DONE] Outline Caption and Menu

![pasted-6](<TO-DO.images/pasted-6.png>)

Add an Add Layer and Delete small buttons in the Caption of the Outline, to make it possible to insert new Layers and to delete the selected objects. Add corresponding commands to the Outline menu.

Done. The Outline's caption has two small buttons, where the picture shows them: Add Layer (a new layer, "Layer2" and so on, above the current one, which it becomes) and Delete (the selected objects; with none selected, the layer chosen in the Outline, with what is on it; a page keeps one layer at least, and the layers of its layout cannot be deleted from the page). Both can be undone. A right-click in the Outline offers the same two commands. The layer operations are checked by a script; the buttons and the menu were not tried by hand.

Q1: By "the Outline menu" I took the menu a right-click in the Outline opens. Did you mean the pane's ▼ menu instead (Float, Dock, Auto Hide...)?

A1: No, let's keep it just to the context menu for now..

Kept as it is: the two commands are in the Outline's right-click menu only.

## View Menu Toggles

For each pane in the application (Outline, History, Tools, Pages, Layouts, etc.), make it possible to toggle its visibility via View menu. 

## Pages: An Ordered List

Pages should be an ordered list, with move up and move down buttons in the caption bar, as well as drag-and-drop supported. The first page in the list is the default entry point for the model.

## Font Family in Properties

Font Family should have a custom editor that brings up a font chooser and sets multiple font-related properties, not just the FontFamily.

## Dirty Indicator

Mark the tab of an unsaved Page that was modified the same way MaxPad marks its dirty document tab. Also, indicate the dirty document by an asterisk in the window caption. Also, create a status bar section where "Modified" can be indicated the way MaxPad does it.

## Outline Glyphs

The leading glyphs in the Outline look like they are disabled, because of the gray elements. Make the glyphs more closely resemble the Tool palette glyphs in style.

## Outline & Tools Background

Should be the same as that of Pages in any theme, including Light and System.

## Settings Toggle to Select Pointer Tool

Add a toggle setting to auto-reset the tool to plain "Select" after each object is placed (without having to press Esc).

## Copy/Paste, Cut/Paste

Make these sequences work. Right now they don't seem to.

## Toolbar Buttons

Add Toolbar buttons in the same style as in PDFTools PDFWorkshop for all commands that it makes sense.

## Navigate Mode in Designer

Make it possible to switch the Designer (toggle with a toolbar button) between Design and Navigate mode. Navigate mode should behave like the Browser. 


## END-OF-TO-DO

