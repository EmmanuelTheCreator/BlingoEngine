# Sample Projects

This folder contains small applications that demonstrate different ways to bootstrap LingoEngine.
Each sample is self-contained and lives under the `Samples/SetupWays` directory.

## SetupWays

| Project | Description |
| --- | --- |
| `LingoEngineMinimalSDL` | SDL bootstrap that configures a 400×300 stage and renders a centered text sprite. |
| `LingoEngineMinimalGodot` | Godot project that uses a `Node2D` script to start the same minimal movie inside the Godot runtime. |
| `LingoEngineWithDirectorInDebugSDL` | SDL bootstrap that switches to the Director tooling (`WithDirectorSdlEngine`) when the build configuration defines `DEBUG`, using TetriGrounds' 730×547 runtime window and 1600×970 Director layout. |
| `LingoEngineWithDirectorInDebugGodot` | Godot project that calls `WithDirectorGodotEngine` in debug builds, matching the same TetriGrounds window sizes so the Director UI fits comfortably. |

### Using the Director toggles

The `*WithDirectorInDebug*` samples reference the Director integration only when the code is compiled with the `DEBUG` symbol.
This keeps release builds lightweight while allowing you to run the Director UI during development.
Update the `DirectorProjectSettings.CsProjFile` path if you copy the sample into your own project structure.
