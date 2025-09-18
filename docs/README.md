# Documentation Overview

This folder collects guides, references, and research notes for BlingoEngine.

## Design & Architecture
- [design/Architecture.md](design/Architecture.md) – Layered architecture and cross‑platform design of the engine.
- [design/RNet.md](design/RNet.md) – End-to-end guide covering every RNet project, transport, and usage pattern.
- [design/Blingo_vs_CSharp.md](design/Blingo_vs_CSharp.md) – Mapping of Lingo syntax to C# equivalents.
- [design/ProjectSetup.md](design/ProjectSetup.md) – Registering the engine and configuring projects and movies.

## Guides
- [BlazorDemo.md](BlazorDemo.md) – Placeholder for notes about the Blazor demo project.
- [CastLibCsvImport.md](CastLibCsvImport.md) – Importing cast libraries and members from CSV files.
- [FilmLoop.md](FilmLoop.md) – Film loop usage, internals, and nesting animations inside animations.
- [Fonts.md](Fonts.md) – Managing fonts across SDL2, Godot, Unity, and Blazor backends.
- [GettingStarted.md](GettingStarted.md) – Repository layout and how to run tests locally.
- [LegacySoundLoading.md](LegacySoundLoading.md) – Notes about `ediM` payloads, format detection, and historical Director sound handling.
- [GodotSetup.md](GodotSetup.md) – Embedding BlingoEngine into a Godot project.
- [Progress.md](Progress.md) – Current implementation status of Lingo language features.
- [SDLSetup.md](SDLSetup.md) – Notes for using the SDL2 front‑end runtime.

## Byte Analysis
- [LegacyShapeRecords.md](../src/BlingoEngine.IO.Legacy/docs/LegacyShapeRecords.md) – QuickDraw shape record layout across classic Director versions.
- [DirDissasembly/Text_Multi_Line_Multi_Style.md](DirDissasembly/Text_Multi_Line_Multi_Style.md) – Offset notes for the sample multi‑style text cast.
- [DirDissasembly/XMED_FileComparisons.md](DirDissasembly/XMED_FileComparisons.md) – Byte differences between sample XMED cast files.
- [DirDissasembly/XMED_Offsets.md](DirDissasembly/XMED_Offsets.md) – Known byte offsets for Director XMED text casts.
- [DirDissasembly/director_keyframe_tags.md](DirDissasembly/director_keyframe_tags.md) – Detailed research into Director keyframe and channel tags.

The [`docfx/`](docfx) subfolder contains the DocFX configuration for generating the project's website.

