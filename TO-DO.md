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

## Pages Line Spacing

Pages list's lines are too far apart. Make them the same spacing as Tools palette. The same applies to Layouts, Parts, Images, and History.

# END-OF-TO-DO