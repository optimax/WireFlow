---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Remember Window Position and Panes

Make sure that the window position, layout (the open panes), and the splitter positions are remembered from run to run.

Done. The Designer saves, as it closes, the window's place and size (and whether it is maximized), and the docking layout: which panes are docked where, floating, auto-hidden or closed, and the sizes the splitters give them. The next run opens the window where it was, before it shows, and puts the panes back. A window last placed on a screen no longer attached opens where Windows puts it. The restore of the old window position never worked (its test was the wrong way round); it does now. The History pane had the same pane ID as Layouts, which a saved layout cannot tell apart; it has its own now. A script saved a layout and read it back (sizes and a closed pane survive); the real window was not tried by hand.

BUG: Splitter positions are note being saved.

The Designer you tried was the build published before this change (19:45): the settings it wrote at 21:26 hold no layout. The build published at 21:48 saves the panes and the sizes the splitters give them (a drag of a splitter sets the size the layout keeps, and a script showed sizes surviving a save and a restore). Please try that one: drag a splitter, close the Designer, start it again.

## [DONE] BUG: Float a Pane Crashes App

Setting a pane to Float, crashes the application.

Fixed. The crash came as a floating pane was dragged, when the Designer shows the drop targets: WPF's Fluent theme gives every window shown without a style of its own Fluent's plain window look, which replaced the look of the docking windows, so the drop-target window lacked its parts. The floating windows were affected too. The docking windows now name their own look before they show. Checked by a script that reproduced the crash with the Designer's themes and then, fixed, showed the drop targets and a floated pane in their own look, light and dark; not by hand.

## BUG: Adding Images to Project

![pasted-8](<TO-DO.images/pasted-8.png>)

There is no way to add an image to the project. Also, OK and Cancel buttons are clipped rendering their text almost invisible.

## BUG: Color Selection Dialog Controls

Color Selection Dialog (Fill, Background, Stroke, etc.) overlaps color component textboxes. Also, it does not provide enough room for the "Web color" box, especially, when the focus is on it and the clear-X appears.

![pasted-4](<TO-DO.images/pasted-4.png>){width="200" style="max-width: 80%"}

Fixed, with the item below. The H, S, V and A values (and R, G, B, L) show in full: the theme's text boxes were wider than the room the sliders give them. The Web color box is wider, so #AARRGGBB shows whole with the clear-X. Checked by drawing the dialog to an image with the light theme; not by hand.

BUG STILL THERE: 

![pasted-7](<TO-DO.images/pasted-7.png>)

The text boxes are not high-enough.

## Stack Panel Behavior

![pasted](<TO-DO.images/pasted.png>)

Place a rectangle inside a vertical StackPanel. --> The rectangle does not span the panel horizontally as it should but seems clipped on the right instead.

Fixed by the conversion to a DOM: a rectangle drawn inside a vertical stack panel now spans it (checked by a script: an 800 px wide panel, a rectangle drawn 430 px wide inside it shows 800 px wide at its left edge, and stays so after its node changes). The rectangle keeps its own width in the file, so it comes back if it is taken out of the panel.

TODO: This is not working. The last rectangle in WireFlow > Page 2 still does not span the width of the stack panel. Apparently, only the last object in StackPanel does not get properly stretched.

## New Layout Dialog Look

The New Layout dialog does not give its controls sufficient height, making the text clipped.

![pasted-9](<TO-DO.images/pasted-9.png>)
## ID Property

In addition to Name, every element should define a common property ID. The ID should be the target/content of an Href link.

Done, under the assumptions below. Every element, and every page and layout, has an ID in Properties (under Common, after Name), empty until you set one; it is saved with the project. A link (Href) can name a page, as before, or an ID: the ID of a page leads to that page, and the ID of an element leads to the page that holds it. This works in the Browser and with the Designer's Navigate tool. Names and IDs compare ignoring case, as page names do. With several objects selected, Properties does not offer ID (each keeps its own), as it does not offer Name. Checked by a script against a copy of a sample project; not by hand.

**Q1: Should a new element get an ID by itself (say `rect-12`, unique in the project), or stay without one until you give it one, as now?**

A1: Give it an automatic one.

**Q2: Should the Designer refuse an ID that another element or page already has? Now nothing checks, and a link goes to the first page found that has it.**

A2: Refuse, if it is not too expensive to check. Otherwise the first found link.

**Q3: When a link names an element's ID, the Browser opens the page holding it. Should it also show which element (scroll it into view, mark it briefly)?**

A3: No. Just open the page.

**Q4: Should links keep naming pages by name, or should they name IDs only? Both work now; a page's name is tried first.**

A4: IDs only. Change the Hrefs of existing projects accordingly.

## Outline Caption and Menu

![pasted-6](<TO-DO.images/pasted-6.png>)

Add an Add Layer and Delete small buttons in the Caption of the Outline, to make it possible to insert new Layers and to delete the selected objects. Add corresponding commands to the Outline menu.

Done. The Outline's caption has two small buttons, where the picture shows them: Add Layer (a new layer, "Layer2" and so on, above the current one, which it becomes) and Delete (the selected objects; with none selected, the layer chosen in the Outline, with what is on it; a page keeps one layer at least, and the layers of its layout cannot be deleted from the page). Both can be undone. A right-click in the Outline offers the same two commands. The layer operations are checked by a script; the buttons and the menu were not tried by hand.

**Q1: By "the Outline menu" I took the menu a right-click in the Outline opens. Did you mean the pane's ▼ menu instead (Float, Dock, Auto Hide...)?**

A1: No, let's keep it just to the context menu for now..

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

## Toolbar Buttons

Add Toolbar buttons in the same style as in PDFTools PDFWorkshop for all commands that it makes sense.

## Navigate Mode in Designer

Make it possible to switch the Designer (toggle with a toolbar button) between Design and Navigate mode. Navigate mode should behave like the Browser. 


## END-OF-TO-DO

