---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [DONE] Convert to .NET 10

Convert the solution to .NET 10.

Done: all projects are SDK-style `net10.0-windows`. Newtonsoft.Json is upgraded to 13.0.4, and `publish-locally.bat` publishes the 64-bit Designer. Both apps start, and a sample project loads and saves. See `HANDOFF.md`.

Question 1: Newtonsoft.Json is still 8.0.3. It has no .NET Core build, so NuGet uses its .NET Framework DLL (warning NU1701), and it has a known high-severity vulnerability, GHSA-5crp-9r3c-p9vr (warning NU1903). Should I upgrade it to 13.0.4? It is also MIT-licensed, and its package is already in `packages/`.

A: Yes.

Question 2: `TO-DO-INSTRUCTIONS.md` says to run `publish-locally.bat` after each task, but there is no such file in the repository. Where is it? Or should I create one, and if so, what should it publish, and to where?

A: Create one. It should publish 64-bit WireFlow Designer to `c:\Programs\Optimax\WireFlow` in Release mode.

## Outline Sync

Adding a new element to a page, for example a rectangle, is not reflected in the Outline view.

# END-OF-TO-DO