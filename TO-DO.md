---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## Lock and Visibility in Outline [DONE]

Each outline element should have a Visible button (a little eye) and a Locked property (a padlock) aligned near the right edge of the Outline that toggle the visibility and lock state of it.

Done. Every row of the Outline that belongs to the page (its layers, and every element on them, a group's children too) has an eye and a padlock, lined up down the right edge of the pane whatever the row's depth. Click the eye to hide or show the element, the padlock to lock or free it. A click on either does not select the row, each click is one step of Undo, and Properties and the page follow at once (a selected element's handles now turn gray the moment it is locked, also when you lock it in Properties).

How they look: an open eye and an open padlock are faint, so the pane stays quiet; a struck-through eye (hidden) and a closed padlock (locked) are drawn plain, so you see at a glance what is hidden or locked. A faint struck-through eye, or a faint closed padlock, means the element is hidden or locked through what holds it (its group, panel or layer), not itself. A long name is cut short with "…" before the buttons.

Things I decided:
- Layers have them too: a layer's eye hides the whole layer with everything on it (in the Browser as well), and its padlock locks everything on it. Nothing can be drawn or pasted on a hidden layer, and the elements selected on a layer are deselected when you hide it.
- A locked element's eye is greyed out, because Locked means its other properties cannot be changed, and Visible is one of them. Unlock it first, then hide it.
- The rows of a layout shown under a page (Layout: Master and what is in it) have no buttons: those elements belong to the layout and are changed in the layout's own tab.
- In Navigate mode the buttons are off, as everything that changes a page is.

Checked by a script in the Designer's window (52 checks, all passing, a real button click among them), and I looked at a drawing of the Outline with the buttons; the Locked and Visible scripts still pass. Not tried by hand.

> The rows of a layout shown under a page (Layout: Master and what is in it) have no buttons: those elements belong to the layout and are changed in the layout's own tab.

TODO: This should be changeable anywhere the elements are shown. 

Done. The rows of a layout shown under a page (its layers and its elements, and those of the layout's own layout) now have the eye and the padlock too, and clicking them changes the layout itself, so the change shows on every page set on that layout. Where it goes:
- If the layout is open in a tab, the change is made there, as an edit of the layout: its tab gets the modified dot, and Undo in the layout's tab takes it back (Undo in the page's tab does not, as the page itself did not change; clicking the eye again does).
- If it is not open, the change is saved into the layout's draft at once, as I already do for links renamed in pages that are not open; Save All then makes it part of a revision.
The page shows the change at once, and the Outline with it. Only the rows naming a page or layout ("Page", "Master") have no buttons. Checked by a script in the Designer's window (26 checks, all passing: a layout closed, open behind the page, and in its own tab; a layout's own layout; Navigate mode); the earlier Outline, Visible and Locked scripts pass. Not tried by hand.

## BUG: Pick Color [DONE]

Create a new tool: Pick color with a typical eydropper glyph, in the same group as Select and Navigate. Clicking with it picks up the color  of fill and stroke and gives it to the next element clicked. If Shift is pressed while clicking on second or subsequent element, the colors are applied to these elements one by one. The initial eyedropper mouse cursor changes to paint bucket when a color is loaded.

Done. The Tools list has a Pick Color tool with an eyedropper glyph, in the first group after Select, Pan and Navigate. Click an element to take its fill and stroke; the cursor changes from an eyedropper to a paint bucket. Click another element to give it those colors; the tool is then empty again and the cursor is the eyedropper. Hold Shift while clicking to keep the colors loaded, so each further element you click gets them, until a click without Shift. Each giving is one step of Undo.

Details you may want to know: an element only gives and gets what its kind has, so a text (which has a background but no stroke) passes on its fill alone, and keeps its own text color; a click on a group's child works on the child; an image, a group itself or the empty page does nothing. The colors are held by the tool, so you can take them on one page and give them on another open tab that has the tool chosen. Choosing the tool deselects, since a selected element's handles would catch the clicks. The two cursors are new files, drawn by a small script from the glyphs (Documents/make-cursors.cs), so they can be redrawn. Checked by a script in the Designer's window (23 checks, all passing, real mouse presses among them; Shift was simulated, as a script cannot hold a key), and I looked at drawings of the glyph and of both cursors; not tried by hand.

TODO: Transferring color with the eyedropper does not seem to be undoable operation.

Fixed. I could not make it fail on a page's own elements: a script choosing the tool in the Tools list, clicking with the mouse and running the window's Undo and Redo found every giving undone and redone, on the page as well as in the file's data, for rectangles, ellipses, texts and boxes, with Shift, and with a color from a chart. It failed on the elements of the page's layout (a header, say): those are drawn from a copy of the layout, so giving one colors changed only that copy. The page showed the new colors and was marked modified, but Undo had nothing to take back, saving did not keep them, and the layout itself never changed.

Now Pick Color gives colors only to the page's own elements, as every other edit on a page works. You can still take colors from a layout's element; a giving click on one does nothing, and the tool keeps its colors for the next click. A layout's colors are changed in the layout's tab. If you would rather have a click on a layout's element on a page color the layout itself, as the Outline's eye now does, say so; Undo in the page's tab would not take that back, though (only the layout tab's Undo would). Checked by scripts in the Designer's window (the case above failed before the fix and passes now; 9 + 24 checks, and the earlier Pick Color and chart scripts pass). Ctrl+Z itself was not pressed, as a script cannot hold Ctrl; the Undo command it runs was. Not tried by hand.

## BUG: Disable Save All [DONE]

When there is nothing to save (all tabs are unchanged).

Done. Save All (the menu item, the toolbar button and Ctrl+Shift+S) is now off when there is nothing to save, and comes on as soon as any open tab has changes, including a tab behind the one in front. One case goes beyond "all tabs unchanged", on purpose: after you press Save on a tab, the tab is unchanged, but what you saved is a draft that is not yet part of a revision (the History pane shows it as "Draft"). Save All stays on then, because making that revision is exactly its job; otherwise a draft could never be committed. The same holds for a page you just created. Once Save All has run, it is off again. Checked by a script in the Designer's window (15 checks, all passing); not tried by hand.

TODO: The only document that's not saved is the current one. Save with the normal Save button. The current document is no longer marked unsaved, and none other is. Yet the Save All button is enabled.

Fixed. Save All is now on only while an open tab has changes not saved, the same rule as the asterisk in the window's caption. In your case it goes off as soon as Save has saved the only changed tab. A page just created, or a layout changed from the Outline's eye while it is not open, no longer turns it on either.

What follows from it: Save writes the tab's draft into the file, and a draft becomes part of a revision (a numbered entry in History) only at the next Save All, which still commits every draft in the file along with the tabs it saves. Nothing is lost meanwhile: drafts stay in the file and are what the pages show, also after closing and reopening. Checked by a script in the Designer's window (16 checks, all passing, your sequence among them); not tried by hand.

## BUG: Switch to a History Point [DONE]

Double click on a History item should revert the page to that point in history — that particular revision.

Done. Double-click a version in the History pane (Draft, a Revision, or First version) and the page in front becomes what it was in that version: its elements, layers, size, background and layout. It is done as an ordinary edit, so it is safe: Undo takes it straight back, the tab shows as modified, and nothing in the file's history is rewritten. When you then save, the reverted page becomes a new revision at the top, and all the earlier ones are still there to go back to. The page keeps its name, and its elements keep the IDs they had, so links still work. It works for layouts too, and the pages set on a layout follow. Three limits: it does nothing in Navigate mode (which is read-only); a layout that an old version was set on is not brought back if the project no longer has it; and it does not work for parts, whose tabs cannot edit the part yet. Checked by a script in the Designer's window on a copy of Data/optimax-old.wf (25 checks, all passing); not tried by hand.

BUG: After switching back to the current revision, if nothing has changed, the current tab should not be marked as modified.

Fixed. The tab now remembers the page as it was when opened or last saved. After a double-click in History, the tab is marked modified only if the page differs from that. So going back to the version the tab shows ("(current)") leaves it unmodified: right away, after you have looked at an older version, and also after an edit, which it drops (Undo brings the edit back, with the modified mark). Undo and Redo use the same comparison, so stepping back onto the saved state shows it unmodified as well. An element from before IDs keeps having none when its version is brought back, so such pages also come back unmodified. Checked by a script in the Designer's window (the History script with these cases added, all passing; the Undo-related scripts pass too); not tried by hand.

## END-OF-TO-DO

