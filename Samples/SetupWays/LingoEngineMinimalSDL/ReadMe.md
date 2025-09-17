# LingoEngine Minimal SDL Sample

← Back to [Samples overview](../../ReadMe.md)

This sample shows the smallest possible SDL bootstrapping code to render a centered text sprite with LingoEngine.

## What the sample demonstrates
- Configures a 400×300 stage and window through `Startup.cs`.
- Registers a project factory that creates a cast library, a single text member, and a sprite to display it.
- Uses the SDL runner without any Director tooling so the game plays immediately.

## Engine registration
```csharp
var services = new ServiceCollection();
IServiceProvider? serviceProvider = null;

services.RegisterLingoEngine(configuration => configuration
    .WithLingoSdlEngine("Minimal SDL Game", MinimalGameSDL.StageWidth, MinimalGameSDL.StageHeight)
    .SetProjectFactory<MinimalGameFactorySDL>()
    .BuildAndRunProject(sp => serviceProvider = sp));
```

`MinimalGameFactorySDL` sets the project folder to the current directory, so the engine looks for assets next to the executable.

## Key files
- `Startup.cs` wires up dependency injection, calls `.WithLingoSdlEngine(...)`, and runs the engine.
- `MinimalGameFactorySDL.cs` sets project settings, creates the cast/text member, and builds the startup movie.
- `MinimalGameSDL.cs` groups the constants shared across the sample.

## Try it out
Execute the project with:

```bash
dotnet run --project Samples/SetupWays/LingoEngineMinimalSDL/LingoEngineMinimalSDL.csproj
```

You should see a 400×300 window with the message “This is a sample project with the minimal setup.” centered on a black background.
