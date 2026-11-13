---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Add Cut/Copy/Paste to Context Menu

Add Cut/Copy/Paste to the Context Menu of the Designer.

Done. The page's right-click menu now starts with Cut, Copy and Paste (with their keys shown), then Duplicate and Delete. They are the Edit menu's own commands, so they are on and off just as those are: Cut and Copy need a selection, Paste needs something copied, and all three are off in Navigate mode. Paste puts the elements where Edit > Paste does (where they were copied from, moved 10 px while they would cover a copy), not at the point clicked. Checked by a script on a copy of Data/WireFlow.wf (11 checks); not by hand.

## [DONE] Navigate Mode in Designer

Make it possible to switch the Designer (toggle with a toolbar button) between Design and Navigate mode. Navigate mode should behave like the Browser. 

Done, with two questions. The toolbar's compass button (also View > Navigate Mode) switches between Design and Navigate mode. In Navigate mode every open page behaves as in the Browser: a click on a link shows its page (in the same tab, or the tab that already shows it), nothing can be selected or changed (Tools and Properties are off; Undo, Paste and the like are disabled), and Back and Forward (toolbar, View menu, Alt+Left and Alt+Right) go through the pages visited. Switching back gives every page the Select tool. Checked by a script that drives the real window; not clicked through by hand.

Q1: The Navigate tool is still in the Tools list (it follows links in Design mode). Remove it now that there is Navigate mode? I left it.

A1: Actually, remove the Toolbar button for now (but not the code behind it, in case we want to surface it again). The way the tool works is sufficient. 

Q2: Do you want a key to switch modes (F6 is free, for example)? I gave it none.

A2: If F5 is free, then make it F5. Otherwise F9. The key should toggle the mode —select/deselect the Navigate tool.

Done as answered. The toolbar's Navigate Mode button is gone (kept in the XAML as a comment, to be put back). F5 is Reload from File, so F9 switches Navigate mode, as does View > Navigate Mode. The Navigate tool is now the mode itself: choosing it in the Tools list turns Navigate mode on (for every open page), and choosing any other tool turns it off with that tool; F9 off gives the Select tool. The Tools list therefore stays usable in Navigate mode; Properties is still off. Checked by a script on the real window; not by hand.

## [DONE] Property Grid Highlight

The highlight of the currently selected row in Properties grid is almost the same color as the grid's headings. Make it a really light yellow (#FFFFF6E1) and provide a way of defining its color in Tools > Options, as well as a toggle whether to display it at all.

Done, as you said in chat: the whole row, and no yellow. The row being edited in Properties is highlighted across its whole width in the Windows accent color, partly transparent, so it shows in Light, Dark and Classic and is clearly not the gray of the headings. Tools > Options has a new Properties page: "Highlight the row being edited" (on by default) and "Highlight color" (the accent, or blue, green, orange, purple, gray). Checked by a script that rendered it in all three themes; not by hand.

## [DONE] Navigate Mode Indicator

Add a colorful indicator to the workspace that would show the navigation mode is on. The color and text color of which is settable via Tools > Options. See rough illustration below (shown in red):

![pasted-10](<TO-DO.images/pasted-10.png>)

Done. While Navigate mode is on, an orange badge reading "Navigate Mode" (white text) hangs from the top edge of each page, at its center, under the tabs, as in your drawing; clicks go through it to the page. Tools > Options has a new Navigate Mode page: "Indicator color" (orange, red, blue, green, purple, teal, yellow, charcoal) and "Indicator text color" (white, black); a color typed into the settings file is offered as Custom. Checked by a script that hosted the window and rendered it; not by hand.

## [DONE] Renaming Pages

Currently, a Page `Name` is more than a field in the page’s JSON. It’s also:

- the key of the page’s row in the .wf file (Documents, by Kind and Name);
- its entry in the project’s page order (PageOrder);

- its item in the Pages list and its tab’s title.

Renaming a page has to change all of these together, and only Engine.RenamePage does that: it renames the row in the file at once and keeps the page’s place in the order. The Pages list’s Rename command (MainDesignerWindow.xaml.cs:2625) uses it. The row was marked read-only in the DOM rework (ebb5494) to prevent changing it. And that's the problem. 

#### Solution:

An internal (hidden), program-assigned, page GUID (no hyphens, no brackets) should be the key in all cases involving saving and ordering. It should not be displayed to the user but used internally as the Unique Page Identifier (UPI) and a key to the database.

The Page Name should be a user-facing feature that allows the user to meaningfully name the page. It also can serve as a target of Href.

Page ID is a short identifier, with hyphens permitted, but not spaces, to also serve as targets of HRefs. Moreover, the page ID should be changeable from the Properties, just like Name and - when changed - should adjust all the Hrefs that contain it. 

All Hrefs should operate on the basis of IDs or names (check ID first). When ID is entered into an Href, it stays as ID; when a name is, it stays as a name. 

Currently rename page works only from the Pages list. Make it work from Properties.

Done. Every page now has a hidden key, a GUID of 32 hex digits, which the .wf file stores pages under, and which the page order, the tabs, the Pages list and Navigate mode's history use. It never changes and is never shown. The page's Name is just its label: it can be set in Properties (no longer read-only) as well as by Rename in the Pages list. A name another page already has, or an empty one, is refused. The page keeps its place and its open tabs. Links resolve an ID first (of a page, or of an element or layer on one), then a page's name. A link keeps what it names: an ID stays an ID, a name stays a name. Changing an ID in Properties (of a page or any element), or renaming a page, changes the links that named the old value. That covers open pages as an edit you can undo, and pages that are not open directly in the file. Checked by a script on a copy of Data/WireFlow.wf (17 checks); not by hand.

Please note: the file format is now 3. A format 2 file is upgraded in place, in one transaction, the first time the new Designer opens it, and an older build can no longer open it afterwards. Undoing a rename made in Properties gives the page its old name back in the page itself, but the Pages list keeps the new name until the page is saved. Layouts and parts are still known by their names; that is the next item.

## [DONE] Renaming Layouts

Currently, renaming a layout isn’t implemented at all. SqliteRepository.RenameLayout throws NotImplementedException (SqliteRepository.cs:296), because pages refer to their layout by name (the page’s Master). A rename would also have to rewrite the Master of every page, and every layout, that uses it. Parts are the same: elements refer to a part by name, and RenamePart throws too.

Make analogous changes as to the Pages, so that renaming layouts is easy. 

NOTE: Any page can become a Layout. Eventually, a Layout could be instantiated as a Page. Normal use for Layouts is to provide the background for a page (or another layout).

Done, as for pages. Layouts now have hidden keys too. A layout can be renamed in Properties (Name); a name another layout has is refused. A page's or layout's Layout (Master) can name a layout by its ID or by its name: an ID is tried first, then a name. Renaming a layout makes the pages and layouts that named its old name name the new one. Changing a layout's ID does the same for those naming its ID. New layouts get an ID made of their name, as new pages do. Two things changed along the way. A layout is now read with its unsaved edits, as a page is, so a new layout opens at once and its edits show before Save Project. And layouts set on each other in a circle no longer hang the drawing: the chain stops at the layout it comes back to. The New Page and New Layout dialogs still list layouts by name. Checked by a script on a copy of Data/WireFlow.wf (15 checks); not by hand. Choosing the Layout from a list in Properties is the next item.

## [DONE] Setting Layout

It should be possible to choose a Layout from a dropdown in Properties, in addition to being able to type its name or ID.

Done. A page's or layout's Layout row in Properties is now a drop-down list of the project's layouts by name, with an empty entry for none. Its box still takes a layout's name or ID typed in, which is kept as typed. A layout chosen is set as the list closes; one typed is set with Enter or when the box loses the focus, and Esc takes the typing back. A name or ID of no layout is refused, and so is a circle: a layout set on itself, or on a layout that is set on it, nearer or farther. The row shows the reason. The page in front shows the layout chosen at once, drawn under its own layers, and loses it when the row is cleared. Checked by a script on a copy of Data/WireFlow.wf (12 checks); not by hand.

## [DONE] Layouts in Pages List

Layouts should not appear in the list of Pages, only in Layouts.

Fixed. The Pages list never listed layouts. Save Project, and Yes when the Designer asks as it closes, saved every open tab as a page. So a layout edited and saved became a page of the same name, and that page showed in the Pages list. Each tab is now saved as what it is: a page as a page, a layout as a layout, a part as a part. Checked by a script on a copy of Data/WireFlow.wf (10 checks); not by hand.

The bug left its mark in Data/WireFlow.wf. Its page "NormalLayout" holds the blue header (a 900 x 52 rectangle) that was drawn on the layout, while the layout "NormalLayout" itself is still empty. The file is yours, so I left it as it is.

Should I repair Data/WireFlow.wf by moving the page's content into the layout and deleting the page? Or will you do it yourself (cut the rectangle from the page, paste it into the layout, then delete the page)?

A: I'll do it myself.

## [DONE] Editing Layouts

Layouts, just like pages, must remain editable. Anything you can do to a page, you can do to a Layout. Changing a Layout must change any open Pages that depend on it.

NOTE: Prevent cycles in Layout dependencies. A Layout pipeline is a chain, never a cycle.

Done. A layout opens in its own tab and is edited there as a page is: the same tools, commands, Outline, Properties, Undo and Cut/Paste. I found nothing a page can do that a layout tab cannot. The pages and layouts open now show a layout as it is being edited, before it is saved. An element drawn, moved, recolored or deleted on the layout, its background, and an undo or redo all show at once on every open page or layout set on it, nearer or farther. Closing the layout's tab without saving takes those edits back off the pages.

Cycles are refused wherever one could be made. Setting a layout was covered already. Now renaming a layout, or giving it an ID, to a name or ID that a layout under it names as its layout is refused too, and so is creating a new layout of such a name. Drawing still stops at a layout already in the chain, in case a file has a cycle. Checked by a script on a copy of Data/WireFlow.wf (11 checks); not by hand.

I noticed one related thing: closing a tab with unsaved changes discards them without asking. That holds for pages, layouts and parts alike, and only Save Project or closing the Designer saves them. It matters more now, since closing a layout's tab also takes its edits off the open pages.

Should closing a tab with unsaved changes ask Save / Don't Save / Cancel, as AGENTS.md describes for dirty documents? (Save would keep the edits as the document's draft, as Save Project does before it commits.)

A: yes.

Done as answered. Closing a page, layout or part tab with unsaved changes now asks "Save changes to the page "Page 2"?" with Yes, No and Cancel. Yes saves it as what it is (as its draft, which Save Project then commits, as before), No drops the changes, and Cancel keeps the tab open. A tab without changes closes without asking. I extended this to closing the project: New Project, Open Project (and the recent files), Reload from File and Close Project dropped every unsaved change silently. They now ask "Save changes to the project?" once, as closing the Designer does, and Cancel stops the command; the tabs then close without asking one by one. Checked by a script on a copy of Data/WireFlow.wf (12 checks); not by hand.

Publishing: the last three items are built (Release, no errors) but not published. publish-locally.bat could not replace c:\Programs\Optimax\WireFlow, because WireFlow Designer is running from there. I did not close it, since it may be yours. I will publish again after the next item; if it is still running then, run publish-locally.bat once you close it.

A mishap on my side: three runs of my test scripts hung while closing their hidden window, on the Designer's "Save changes?" prompt, and were killed before they could put your settings file back. %APPDATA%\WireFlow\WireFlow.Designer.config kept your theme, colors and pane layout. It lost your window place and size (they are the test window's now: Windows will place it again), and Reopen the previous project at startup is off. I cleared the previous project and tabs, which pointed at a temporary test file. Please switch that option back on in Tools > Options if you had it on. The scripts now keep a backup of the settings and cannot hang on that prompt.

## [DONE] Highlight Link in Navigate

Make a toggle switch (and a Toolbar button) to highlight all links on a page to make them visible. Highlight them with a semi-transparent overlay, the color and opacity of which can be adjusted with Tools > Options.

Done. View > Highlight Links (Ctrl+L) and a toggle on the toolbar, after Back and Forward, lay a semi-transparent overlay over every link on the pages open. That covers the page's own elements, those inside groups and stack panels, part references, and the elements of its layouts. The overlay follows the page: a link made or taken away, an undo, another page shown, or a layout redrawn lays the overlays again. Clicks go through them. Tools > Options > Navigate Mode has Link highlight color (Blue by default, also Yellow, Green, Orange, Red, Purple, Teal, Gray) and Link highlight opacity (20% to 60%, 35% by default).

Assumptions I made: it works in Design mode too, not only in Navigate mode, since it is just as useful when checking links while editing. Its on or off state is kept in the settings. Its key is Ctrl+L, which nothing else used. Checked by a script on a copy of Data/WireFlow.wf (10 checks); not by hand.

## [DONE] Zoom Controls and Fit Page

Add zoom controls to the toolbar and commands to the menu, similar to those in MarkdownTools. Also, add a "Fit Page" button and command to zoom-to-fit.

Done, as in MaxWrite. View has Zoom In (Ctrl+=), Zoom Out (Ctrl+-) and Actual Size (Ctrl+Shift+0), in steps of 10%, and Fit Page (Ctrl+0), as in PDFTools. The toolbar ends with the same group as MaxWrite's: Zoom Out, the zoom of the page in front ("100%"; a click goes back to Actual Size), Zoom In, and then Fit Page, with PDFTools' magnifier and page icons. Zooming keeps the point at the center of the view in place; the mouse wheel still zooms about the point under the mouse, in the same steps. Fit Page shows the whole page as large as it fits, with a little room around it, and centers it.

Assumptions I made: each tab has its own zoom, and a tab opens at 100% (the zoom is not kept in the settings). The zoom goes from 10% (low enough for Fit Page to show a long page whole) to 500%; MaxWrite stops at 25%. Fit Page zooms once; it does not keep fitting as the window is resized. Ctrl+0 for Fit Page is the usual key for it in PDF viewers; nothing in the Designer used it. Checked by a script on a copy of Data/WireFlow.wf (19 checks); not by hand.


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

## [DONE] View Menu Toggles

For each pane in the application (Outline, History, Tools, Pages, Layouts, etc.), make it possible to toggle its visibility via View menu. 

Done. The View menu lists every pane under Theme (Tools, Pages, Layouts, Parts, Images, History, Outline, Properties), checked while it shows; choosing one hides or shows it, and a pane shown again goes back where it was. Each pane's caption X now hides it too (it was disabled), and the View menu brings it back. A hidden pane stays hidden next time, as part of the remembered layout. Checked by a script; not by hand.

## [DONE] Pages: An Ordered List

Pages should be an ordered list, with move up and move down buttons in the caption bar, as well as drag-and-drop supported. The first page in the list is the default entry point for the model.

Done. The Pages list keeps the order you give it, saved in the project: the Pages caption has Move Up and Move Down buttons for the selected page, and a page can be dragged to its place (a line shows where it goes). The first page is where the project starts, in the Designer and the Browser. A project never ordered starts with its home page first and the others by name, so it starts where it did. A new page comes last, and a renamed page keeps its place. The order is saved at once, as adding, deleting and renaming pages are, not at Save Project. Checked by a script; the buttons and dragging not by hand.

## [DONE] Font Family in Properties

Font Family should have a custom editor that brings up a font chooser and sets multiple font-related properties, not just the FontFamily.

Done. FontFamily in Properties keeps its text box and has a "…" button (Ctrl+Enter or a double click on the value open it too) that opens the font dialog of MaxPad and MaxWrite (OptimaxTools.FontSelection, now in WireFlow's LocalPackages), on the element's font. OK sets the family, the size, and the face: its weight, stretch and style. Texts and boxes did not have italic; they have a FontStyle now (Normal, Oblique, Italic), shown under Text and saved with the page, so an italic face chosen is kept. With several elements selected, each gets the font. Checked by a script; not by hand.

## [DONE] Dirty Indicator

Mark the tab of an unsaved Page that was modified the same way MaxPad marks its dirty document tab. Also, indicate the dirty document by an asterisk in the window caption. Also, create a status bar section where "Modified" can be indicated the way MaxPad does it.

Done. A page, layout or part with changes not saved has a small orange dot after its name in its tab, as MaxPad's tabs have (the same size and color, read off MaxServices.Core's tab). The window caption has an asterisk after the project's name, "[Project]* WireFlow Designer - folder", while any page open has changes not saved: the caption names the project, not a page, so the asterisk says that something in it needs saving. The status bar's first section says Modified or Saved of the page in front, as MaxPad's does. Checked by a script (the dot); the caption and the status bar not by hand.

## [DONE] Outline Glyphs

The leading glyphs in the Outline look like they are disabled, because of the gray elements. Make the glyphs more closely resemble the Tool palette glyphs in style.

Done. The Outline's entries now have the Tools palette's line glyphs, drawn as the palette draws them, in the row's own text color, so they follow the theme and no longer look disabled: a rectangle, ellipse, text, box, image and stack panel have their tools' glyphs, and layers, layouts, groups and parts new ones in the same style. Checked by a script; not looked at by hand.

## [DONE] Outline & Tools Background

Should be the same as that of Pages in any theme, including Light and System.

Done. The Tools list and the Outline now have the background Pages has, in every theme: they took the theme's own list background before, which differs from it in Light and System. Checked by a script in Light and Dark; not looked at by hand.

## [DONE] Settings Toggle to Select Pointer Tool

Add a toggle setting to auto-reset the tool to plain "Select" after each object is placed (without having to press Esc).

Done. Tools > Options has a Drawing page with "Return to the Select tool after drawing an object", off by default (the Designer behaves as before until it is turned on). When on, the tool goes back to Select as soon as an object is drawn, as Esc does. Checked by a script; not by hand.

## [DONE] Copy/Paste, Cut/Paste

Make these sequences work. Right now they don't seem to.

Done. The Edit menu's Cut, Copy and Paste were placeholders with no command; they now work, with Ctrl+X, Ctrl+C and Ctrl+V. Paste puts the elements where they were copied from (on another page, the same place), or 10 px down and right while they would lie on their originals, and selects them; each gets a new ID, and Undo takes a paste back. A text box being typed in keeps Ctrl+X/C/V for its own text. Checked by a script; not by hand.

## [DONE] Toolbar Buttons

Add Toolbar buttons in the same style as in PDFTools PDFWorkshop for all commands that it makes sense.

Done. A toolbar under the menu, in PDF Workshop's style (flat buttons with a frame under the mouse, line icons, the theme's colors, lines above and below): New, Open (PDF Workshop's folder and its name), Save | Undo, Redo | Cut, Copy, Paste, Delete | Group, Ungroup, Bring to Front, Send to Back | Align Left, Center Horizontally, Align Right, Align Top, Center Vertically, Align Bottom | Distribute Horizontally and Vertically, Make Same Width and Height. Each runs its menu command and is dimmed when that cannot run; tooltips name the keys. The hidden, unused old toolbar is gone. Checked by rendering it in Light, Dark and Classic; not clicked through by hand.

## END-OF-TO-DO

