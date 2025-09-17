# SDL Sample with Director in Debug Builds

← Back to [Samples overview](../../ReadMe.md)

This project adds the Director tooling only when compiled with the `DEBUG` symbol.
In release builds it behaves like the minimal SDL sample.

## What the sample demonstrates
- Conditional project reference to `BlingoEngine.Director.SDL2` in the `.csproj` file.
- Conditional use of `.WithDirectorSdlEngine(...)` versus `.WithBlingoSdlEngine(...)` inside `Startup.cs`.
- Reuses the minimal project factory to render a centered text sprite while showing how to expose `DirectorProjectSettings`.
- Uses the same 730×500 stage dimensions as TetriGrounds so the Director UI has enough space to render.

## Engine registration
```csharp
var services = new ServiceCollection();
IServiceProvider? serviceProvider = null;

services.RegisterBlingoEngine(configuration =>
{
#if DEBUG
    configuration = configuration.WithDirectorSdlEngine(
        "SDL Director Sample",
        MinimalDirectorGame.DirectorWindowWidth,
        MinimalDirectorGame.DirectorWindowHeight,
        director =>
        {
            director.CsProjFile = "BlingoEngineWithDirectorInDebugSDL.csproj";
        });
#else
    configuration = configuration.WithBlingoSdlEngine(
        "SDL Director Sample",
        MinimalDirectorGame.RuntimeWindowWidth,
        MinimalDirectorGame.RuntimeWindowHeight);
#endif

    configuration
        .SetProjectFactory<MinimalDirectorGameFactorySDL>()
        .BuildAndRunProject(sp => serviceProvider = sp);
});
```

## Conditional project reference
```xml
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
  <ProjectReference Include="..\..\..\src\Director\BlingoEngine.Director.SDL2\BlingoEngine.Director.SDL2.csproj" />
</ItemGroup>
```

`MinimalDirectorGameFactorySDL` keeps the project folder at the current directory so the relative `CsProjFile` path resolves during development.

## Key files
- `Startup.cs` switches between the Director runner and the regular SDL runner based on `#if DEBUG`.
- `MinimalDirectorGameFactorySDL.cs` configures project settings, builds the cast, and creates the startup movie.
- `MinimalDirectorGame.cs` keeps the constants shared across the sample.

## Try it out
Compile in release mode to run the plain SDL version:

```bash
dotnet run --configuration Release --project Samples/SetupWays/BlingoEngineWithDirectorInDebugSDL/BlingoEngineWithDirectorInDebugSDL.csproj
```

Compile in debug mode to launch the Director-enabled window (1600×970):

```bash
dotnet run --configuration Debug --project Samples/SetupWays/BlingoEngineWithDirectorInDebugSDL/BlingoEngineWithDirectorInDebugSDL.csproj
```

