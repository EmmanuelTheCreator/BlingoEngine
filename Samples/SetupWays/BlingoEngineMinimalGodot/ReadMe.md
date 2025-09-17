# BlingoEngine Minimal Godot Sample

â† Back to [Samples overview](../../ReadMe.md)

This Godot 4.5 project mirrors the SDL sample but runs inside the Godot editor/runtime using a simple `Node2D` scene.

## What the sample demonstrates
- Uses a Godot script (`Scenes/MinimalGameRoot.cs`) to initialize dependency injection and call `.WithBlingoGodotEngine(this)`.
- Reuses the same minimal project factory to create a 400Ã—300 stage with a centered text sprite.
- Shows how to control the Godot window size so it matches the stage dimensions.

## Engine registration
```csharp
var services = new ServiceCollection();
services.RegisterBlingoEngine(configuration => configuration
    .WithBlingoGodotEngine(this)
    .SetProjectFactory<MinimalGameFactoryGodot>()
    .BuildAndRunProject(sp => _serviceProvider = sp));
```

`MinimalGameFactoryGodot` keeps the project folder in the current directory, making it easy to bundle assets with the Godot scene.

## Key files
- `Scenes/MinimalGameRoot.cs` bootstraps the engine inside the Godot scene tree and disposes services on exit.
- `MinimalGameFactoryGodot.cs` configures project settings, cast members, and builds the startup movie.
- `project.godot` + `Scenes/minimal_game_root.tscn` describe the minimal Godot project structure.

## Try it out
Open the folder in Godot 4.5, load `Scenes/minimal_game_root.tscn`, and press **Play**. The scene displays the â€œminimal Godot setupâ€ text centered on a black background.

