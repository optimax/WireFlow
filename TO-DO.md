---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## BUG: WireFlowRectangle

Placing a Rectangle component on the page makes a "WireFlowRectangle" node appear in the Outline. It should just be "Rectangle".

## [DONE] BUG: Renaming Page

Attempting to rename a page in the Pages list seems to work in some places, but in that list, the name of the page does not change in the list. Moreover, the page remains selected and there is no way to deselect it any more. The image below shows Main page having been renamed to HOME - but it does not show, and now two pages are selected in the list.

![pasted-12](<TO-DO.images/rename-page-bug.png>)

Additionally, links that targeted the old name (Main) are now broken throughout the model.

Fixed. Renaming a page in the Pages list now shows the new name in the list at once, and leaves just that page selected; another page can be selected afterwards. The list replaced the page's entry with an identical one, which WPF handles badly: the row kept its old text and the selection stuck. It now redraws its rows instead. The same fix applies to the Layouts list.

About the links: in your Data/WireFlow.wf the rename did reach them (the layout's link now names HOME, and it leads to the page). What I found is that your HOME page has no ID, unlike the others: a new project's first page was created without one, so links to it could only use its name. A new project's first page now gets an ID too. Existing pages without one keep working by name; give HOME an ID in Properties if you want links to survive renames. If you still see broken links after a rename, please tell me which link and where. Checked by a script on a copy of Data/WireFlow.wf (9 checks); not by hand.

## [DONE] Esc Should Work Anywhere

Pressing Esc should deselect the current tool and return to Select mode regardless of which pane has focus, not only in the middle Designer pane. This includes Navigate mode/tool.

Done. Esc now gives the Select tool wherever the keyboard is: the page, Tools, Pages, Outline, Properties and the other panes, docked or floating. It works even when the control with the keyboard uses Esc itself, so Esc ends a rename or closes a drop-down and also returns to the Select tool. In Navigate mode, Esc leaves the mode, with the Select tool (it used to keep the Navigate tool). Checked by a script on a copy of Data/WireFlow.wf (8 checks); floating panes and the real keyboard were not tried.

## [DONE] BUG: Save As

Save As does not work.

Done. File > Save As now copies the whole project (pages, layouts, parts, images and its earlier revisions) to the file you choose, switches to that file (the window's caption and the recent files follow), and saves every open tab there as Save All does. The file you started from keeps what it had: changes not yet saved go into the new file only. Choosing the same file just saves the project. Checked by a script on a copy of Data/WireFlow.wf (10 checks); not tried by hand.

## [DONE] BUG: Options | Navigate Mode

![pasted-12](<TO-DO.images/pasted-12.png>)

The color selections for Navigate Mode in Options should be regular color pickers with swatches, not a dropdown with names.

Done. Indicator color, Indicator text color and Link highlight color are now buttons showing the color as a swatch and as #AARRGGBB; a click opens the Designer's color dialog (the one Properties uses, with its swatches and the colors chosen last), and OK in Options keeps what they show. Link highlight opacity is still a list, as it is not a color. Checked by a script that shows the dialog off-screen (9 checks) and draws the page; the color dialog was not opened by the script, and nothing was tried by hand. The next item, Navigate Mode Color Definitions, will make these the defaults and keep a project's own colors in the project.

## [DONE] BUG: Pane Buttons

![pasted-13](<TO-DO.images/invisible-caption-buttons-bug.png>)

The buttons that should appear in the captions of side panels are invisible in dark themes.

Fixed. The Outline's and the Pages' caption buttons drew their glyphs in black in every theme, which a dark caption hid; they now take the caption's text color, so they are white in Dark. Under the mouse they show the toolbar's frame (before, their background turned the glyph's own color, which hid it too), and while disabled they are dimmed to half rather than to a third. Checked by a script in Dark, Light and Classic, which compared the colors and drew the captions; not tried by hand.

## [DONE] Navigate Mode Color Definitions

The ones that are define in Tools|Options (global app settings) are the defaults, but the options should be saved as part of the document itself, since the colors depend on the colors in the model, to be visible and sufficiently contrasting.  

If a model is open, Tools|Options adjust the model's colors.

If Tools|Options is open without a document, whatever is set there defines the defaults for new documents/models.

Done. A project now keeps its own Navigate Mode colors in its file: the indicator, its text, and the link highlight with its opacity. With a project open, Tools > Options shows and changes that project's colors, and writes them into its file at once (no Save needed). With no project open, Options sets the defaults, which a new project starts with as its own. A project made before this has no colors of its own and shows the defaults until you set them in Options. The Navigate Mode page of Options says which colors it is showing. Checked by a script that drove Options with and without a project open (12 checks); creating a new project was not run by the script, and nothing was tried by hand.

## [DONE] Save All Toolbar Button

Add "Save All" button to the Toolbar.

Done. The toolbar has a Save All button right after Save, drawn as two diskettes, one behind the other. It does what File > Save All does (Ctrl+Shift+S): it saves every open tab that has changes and commits the project as a new revision, and it is on while a project is open. Checked by a script that drew the toolbar in Light, Dark and Classic (22 checks, all passing); the button was not clicked in the running Designer.

## [DONE] Align: Fill Space

Add to the available Alignment options: Fill Space, that makes the selected control(s) fill the available vertical and horizontal space.

Done. Alignment > Fill Space (in the main menu, the right-click menu and on the toolbar after Align Bottom) grows each selected element until its edges meet the nearest elements beside, above and under it, or the edges of the page. A layout's header and footer and the elements of other layers count, so an element drawn between a header, a footer and a sidebar fills exactly the room between them. Elements it overlaps do not stop it (a background under it, a label on it). With several selected, each fills in turn, so two do not take the same room. Inside a group it fills up to the group's other children; a stack panel's child is left alone, as the panel sizes it. It is one step of Undo. Fill Height and Fill Width, further down, will use the same rule one way only. Checked by a script (30 checks, all passing: the calculation alone, and the command in the Designer's window on a copy of Data/WireFlow.wf); not tried by hand.

## [DONE] Cancel Navigate Mode

![pasted-11](<TO-DO.images/pasted-11.png>)

Add an "x" glyph to navigate mode indicator to cancel navigate mode and return to Select tool.

Done. The Navigate Mode indicator ends with an "x", in the color of its text. A click on it leaves Navigate mode and gives the Select tool, as Esc does, on every open tab. Under the mouse the "x" shows a faint round spot, and its tooltip reads "Leave Navigate Mode (Esc)". Clicks on the rest of the indicator still go through to the page under it, so it does not get in the way of links there. Checked by a script in the Designer's window (16 checks, all passing, with mouse presses raised on the "x"), and I looked at a drawing of the indicator in two color settings; not clicked by hand.

## [DONE] Renaming Pages Continued

Undoing a rename made in Properties gives the page its old name back in the page itself, but the Pages list keeps the new name until the page is saved. Sync all changes so that the display is consistent in real time.

Done. Undo and Redo of a rename now reach everything at once: the page itself, the project, the Pages list, the tab, and the links that name the page, in the other open tabs and in pages that are not open. It is the same whether the page was renamed in Properties or in the Pages list. The same holds for a layout's name (the Layouts list, its tab, and the pages and layouts set on it by name) and for an element's or page's ID (the links naming it follow the undo). One case cannot be undone: if another page has taken the old name in the meantime, Undo leaves the page with its current name, in the page and in the list alike, since two pages cannot share a name. Checked by a script in the Designer's window on a copy of Data/WireFlow.wf (20 checks, all passing); not tried by hand.

## [DONE] Font Dialog: Foreground Color

Add Foreground Color selector to the Font dialog. Make the dialog wider than it is tall (dialogs should always strive to present in landscape rather than portrait orientation).

Done. The Font dialog has a Color button, showing the text's color as a swatch and as #AARRGGBB; a click opens the Designer's color dialog (the one Properties uses, with its swatches and recent colors). OK sets the element's Foreground along with the font, and only when you chose another color, so several selected elements with different colors keep their own unless you pick one. The sample is drawn in the color chosen, on the color the text lies on (the element's own fill, else its page's background), so white text for a dark page can be read in a light dialog. The dialog is now 820 x 378 instead of 550 x 538: the sample sits to the right of the lists, as tall as they are, with the Color button above it, and a long sample wraps. I also recorded "dialogs are wider than tall" as a rule in implementation.md. The dialog itself comes from the OptimaxTools.FontSelection package, whose source is not on this machine, so the Designer rearranges it rather than the package changing. Checked by a script (22 checks, all passing) and by looking at drawings of the dialog in Light and Dark; the color dialog was not opened from it by the script, and nothing was tried by hand.

## [DONE] Font Dialog Lists

The Font dialog's lists (Font, Style, Size) have their items way too far apart. Make the lines 100% closer (eliminate almost all the line spacing). This applies in general in this project: whenever you display a list, it needs to have its items tightly spaced.

Done. The rows of the Font dialog's lists are now 24 px (Font) and 20 px (Style, Size) instead of 46 and 42: a row is its line of text plus 4 px. The Size list shows 11 sizes at once where it showed 5. Rows stay tight as the lists scroll, in Light and Dark; Classic's rows were close already and keep the theme's own. I recorded the rule (every list has close rows) in implementation.md and made one helper for it, which a new list calls. Going through the other lists: the image dialog's list was still at the wide spacing and is now compact, like the Images pane. The lists in the main window's panes (Tools, Pages, Layouts, Parts, Images, History) keep the 27 px rows they have had since the "compact lists" item; if you want those as tight as the Font dialog's, say so and it is a one-line change. Checked by a script in Light, Dark and Classic (15 checks, all passing) and by looking at a drawing of the dialog; not tried by hand.

## [DONE] Panel Container Element

Create a regular panel container, in which children (any other element) can be aligned based on their Alignment property (add Alignment to the base object): None, Top, Bottom, Left, Right, Fill. The Alignment property will be settable either by a dropdown listing the values (enumeration) or by a custom editor that pops up with the 9-buttons. 

Done. The Tools list has a new Panel tool. A panel is a container: an element drawn starting inside it becomes its child, where it was drawn. Every element now has an Alignment property (None, Top, Bottom, Left, Right, Fill), listed in Properties under Layout as a drop-down, for one element or several. Only a panel acts on it, and it places its children as a Delphi panel does: Top children first, across the top at their own height, one under the other; then Bottom; then Left and Right, down the room left between them at their own width; Fill takes what remains; None stays where you put it. The order of the children (Bring to Front, Send to Back; the Outline lists them in that order) decides which of two Top children comes first. Resize the panel, or change a child's height or width, and the others follow. An aligned child cannot be dragged or nudged (the panel places it); one with no alignment can. Panels nest, so a panel aligned Left inside another gives you a column with its own header. Ungroup releases the children where they are. Panels are saved, undone, copied and shown in the Browser like stack panels.

Two choices to tell you about. I made the drop-down, not the 9-button popup, since the six values fit a list exactly; nine buttons would need a meaning for the four corners. And on a page itself (outside a panel) Alignment does nothing; if you want a page's own elements to align to the page as well, that is a small addition. Checked by a script (43 checks, all passing: the placement rule alone, the saved file, and the Designer's window drawing a panel and its children, with stack panels checked again); not tried by hand.

## [DONE] Setting Layout

It should be possible to choose a Layout from a dropdown in Properties, in addition to being able to type its name or ID.

Done. A page's or layout's Layout row in Properties is now a drop-down list of the project's layouts by name, with an empty entry for none. Its box still takes a layout's name or ID typed in, which is kept as typed. A layout chosen is set as the list closes; one typed is set with Enter or when the box loses the focus, and Esc takes the typing back. A name or ID of no layout is refused, and so is a circle: a layout set on itself, or on a layout that is set on it, nearer or farther. The row shows the reason. The page in front shows the layout chosen at once, drawn under its own layers, and loses it when the row is cleared. Checked by a script on a copy of Data/WireFlow.wf (12 checks); not by hand.

TODO: Empty items should not be shown in the dropdown. It should be a dropdown box (typing allowed), not a dropdown list (select only). I specifically said "being able to type".

Fixed, and you were right: the box could not be typed in. It was set up as editable, but the property grid draws its own compact drop-down, and that drawing had no text box in it, so every "editable" list in Properties was select-only; the earlier check set the text from code and never noticed. The grid's drop-down now has a real text box when it is editable: click in the Layout row and type a layout's name or ID, Enter (or leaving the row) sets it, Esc takes the typing back, and the arrow at the end opens the list. The list holds the layouts only, with no empty item; to set no layout, empty the box. Ordinary lists in Properties (Alignment, TextWrapping and the like) still open from anywhere on them. Checked by a script in the Designer's window that types into the box key by key (19 checks, all passing), and I looked at a drawing of the row; not tried by hand.

## [DONE] Fit Height, Fit Width

Sizing submenu offers Make Same Height and Make Same Width. Add Fit Vertically and Fit Horizontally:

- Fill Height: Places the element such that it completely fills the *available vertical space* in its container so, if invoked between a header and a footer rectangle, it would fill the remaining space between the header and footer.
- Fill Width:  Places the element such that it completely fills the *available horizontal space* in its container.

Done. The Sizing submenu (in the main menu and the right-click menu) now has Fill Width and Fill Height, after Make Same Size. Fill Height moves the element's top and bottom edges out to the nearest elements above and under it, or to the page's edges, and leaves its left and width alone, so between a header and a footer it fills exactly the space between them; Fill Width does the same sideways. They follow the rule of Alignment > Fill Space (done earlier in this list), which is both at once: Fill Width followed by Fill Height gives the same result. I named them Fill Width and Fill Height, as your two bullets do, rather than Fit Vertically and Fit Horizontally. They are in the menus only; I did not add toolbar buttons, because the toolbar is already about 1300 px wide and a narrower window cuts off its right end (the zoom buttons). Checked by a script in the Designer's window (10 checks, all passing); not tried by hand.

## [DONE] Snap to Other Elements

When moving or resizing an element (with the mouse), make it snap to the edges of other elements, including the page edge.

Done. While you drag an element, its edges snap to the edges of the other elements and of the page once they come within 6 pixels on the screen (so the distance on the page shrinks as you zoom in). It holds on the line while the mouse stays near and lets go when you drag on. A thin pink line across the page shows what it snapped to, and goes when you let go. Left or right edges snap to left and right edges, top or bottom to top and bottom, and both ways at once. Resizing snaps the edge or corner you drag, by the same rule. Elements of the page's layout (a header, a footer) and of other layers count; inside a group or a panel it snaps to the other children and the group's or panel's own edges. Several elements dragged together snap as one block. Hold Ctrl while dragging to move or resize freely, without snapping. Arrow-key moves do not snap. Checked by a script that drives the Designer's own mouse handling and resize handles (35 checks, all passing); the Ctrl key could not be held by the script, and nothing was tried by hand.

## [DONE] Stack Panel "Stacking-From"

Add a "Stacking-From" property to the StackPanel, with values `Beginning`, and `Ending`.

`Beginning` Stacking behaves like today. `Ending` stacking starts the stacking from the opposite end of the panel, so a vertical stack panel with `Ending` stacking would be stacking from the bottom.

Done. A stack panel has a StackingFrom property in Properties, under Layout beside Direction: Beginning (as today) or Ending. With Ending, a vertical panel stacks from the bottom: the first child lies against the bottom edge and each next one above it; a horizontal panel stacks from the right. Resize the panel and the children stay against that end. It is saved with the page, shown the same in the Browser, and existing stack panels are Beginning. I read "stacking from the bottom" as the first child at the bottom and the pile growing upward; if you meant the children in their usual top-to-bottom order, only pushed against the bottom, say so and I will change it.

One fix came with it: after a stack panel's Direction was changed (or that change undone), its children kept the stretched size the other direction had given them until the page was reloaded, so the Designer showed something the Browser did not. They now always take their own size. Checked by a script (17 checks, all passing); not tried by hand.

## [DONE] Show History

Fill the History list with the saved versions.

Done. The History pane now lists the saved versions of the page, layout or part in front, newest first: "Draft" when it has edits saved with Save but not yet made a revision by Save All; "Revision N" for each project revision in which it was saved anew; and "First version" at the bottom. The one the tab shows is marked "(current)". The list follows the tab in front and updates on Save and on Save All; a renamed page keeps its history. Changes you have not saved are not a version and do not show.

Two things to know. The project file keeps no dates, so versions can only be named by revision number; adding a date to each revision is possible, but it is a small addition to the file format, so I did not do it unasked. And the entries do nothing yet when clicked: you asked for the list to be filled, and opening or restoring an earlier version would be the natural next item if you want it. Checked by a script in the Designer's window on a copy of Data/optimax-old.wf (26 checks, all passing); not tried by hand.

## [DONE] Duplicate Behavior

When adding a page with the same name as an already existing one, the dialog is dismissed first and disappears, and only then the error message pops up. Pop up the duplicate error message before dismissing the dialog (on validation), and let the dialog remain for the user to correct the duplicate name.

Done. New Page now checks the name when you press OK: if another page has it, the message appears over the dialog, and the dialog stays open with the name selected, ready to be typed over. The same name in other letter case, or with spaces after it, counts as the same. Cancel after a refusal makes nothing. New Layout works the same way (a taken name, or one that would set layouts on each other in a circle), and so does New Part, which did not check for a duplicate name at all before. Checked by a script that drove the three commands and their real dialogs on a copy of Data/optimax-old.wf (17 checks of their behaviour, all passing). To do that the script had to open the real dialogs, and they showed for a moment at the left edge of your screen, twice; apart from that, nothing was tried by hand.

## END-OF-TO-DO

