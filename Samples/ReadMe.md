# Sample Projects

This folder contains small applications that demonstrate different ways to bootstrap LingoEngine.
Each sample is self-contained and lives under the `Samples/SetupWays` directory.

## SetupWays

| Project | Description |
| --- | --- |
| `LingoEngineMinimalSDL` | SDL bootstrap that configures a 400×300 stage and renders a centered text sprite. |
| `LingoEngineMinimalGodot` | Godot project that uses a `Node2D` script to start the same minimal movie inside the Godot runtime. |
| `LingoEngineWithDirectorInDebugSDL` | SDL bootstrap that switches to the Director tooling (`WithDirectorSdlEngine`) when the build configuration defines `DEBUG`. |
| `LingoEngineWithDirectorInDebugGodot` | Godot project that calls `WithDirectorGodotEngine` in debug builds and the plain runtime otherwise. |

### Using the Director toggles

The `*WithDirectorInDebug*` samples reference the Director integration only when the code is compiled with the `DEBUG` symbol.
This keeps release builds lightweight while allowing you to run the Director UI during development.
Update the `DirectorProjectSettings.CsProjFile` path if you copy the sample into your own project structure.
