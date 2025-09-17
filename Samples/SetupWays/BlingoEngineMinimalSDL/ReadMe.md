# BlingoEngine Minimal SDL Sample

â† Back to [Samples overview](../../ReadMe.md)

This sample shows the smallest possible SDL bootstrapping code to render a centered text sprite with BlingoEngine.

## What the sample demonstrates
- Configures a 400Ã—300 stage and window through `Startup.cs`.
- Registers a project factory that creates a cast library, a single text member, and a sprite to display it.
- Uses the SDL runner without any Director tooling so the game plays immediately.

## Engine registration
```csharp
var services = new ServiceCollection();
IServiceProvider? serviceProvider = null;

services.RegisterBlingoEngine(configuration => configuration
    .WithBlingoSdlEngine("Minimal SDL Game", MinimalGameSDL.StageWidth, MinimalGameSDL.StageHeight)
    .SetProjectFactory<MinimalGameFactorySDL>()
    .BuildAndRunProject(sp => serviceProvider = sp));
```

`MinimalGameFactorySDL` sets the project folder to the current directory, so the engine looks for assets next to the executable.

## Key files
- `Startup.cs` wires up dependency injection, calls `.WithBlingoSdlEngine(...)`, and runs the engine.
- `MinimalGameFactorySDL.cs` sets project settings, creates the cast/text member, and builds the startup movie.
- `MinimalGameSDL.cs` groups the constants shared across the sample.

## Try it out
Execute the project with:

```bash
dotnet run --project Samples/SetupWays/BlingoEngineMinimalSDL/BlingoEngineMinimalSDL.csproj
```

You should see a 400Ã—300 window with the message â€œThis is a sample project with the minimal setup.â€ centered on a black background.

