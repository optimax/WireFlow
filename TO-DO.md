---
Title: TO-DO
---

# TO-DO WireFlow 

> **IMPORTANT**: Follow the instructions found in [`TO-DO-INSTRUCTIONS.md`](TO-DO-INSTRUCTIONS.md).
>
> ---

## [QUESTION] Convert to .NET 10

Convert the solution to .NET 10.

Done: all projects are SDK-style `net10.0-windows`. Debug and Release build with 0 errors. Both apps start, and a sample project loads and saves. See `HANDOFF.md`.

**Question 1: Newtonsoft.Json is still 8.0.3. It has no .NET Core build, so NuGet uses its .NET Framework DLL (warning NU1701), and it has a known high-severity vulnerability, GHSA-5crp-9r3c-p9vr (warning NU1903). Should I upgrade it to 13.0.4? It is also MIT-licensed, and its package is already in `packages/`.**

**Question 2: `TO-DO-INSTRUCTIONS.md` says to run `publish-locally.bat` after each task, but there is no such file in the repository. Where is it? Or should I create one, and if so, what should it publish, and to where?**


# END-OF-TO-DO