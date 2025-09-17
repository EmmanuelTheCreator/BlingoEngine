# Godot Sample with Director in Debug Builds

This Godot 4.5 project mirrors the SDL Director sample, but toggles the Director tooling while running inside the Godot editor/runtime.

## What the sample demonstrates
- Conditional project reference to `LingoEngine.Director.LGodot` in the `.csproj` file.
- Conditional use of `.WithDirectorGodotEngine(...)` or `.WithLingoGodotEngine(...)` inside the root node script.
- The same minimal project factory pattern that renders a centered text member.

## Engine registration
```csharp
var services = new ServiceCollection();
services.RegisterLingoEngine(configuration =>
{
#if DEBUG
    configuration = configuration.WithDirectorGodotEngine(this, directorSettings =>
    {
        directorSettings.CsProjFile = "LingoEngineWithDirectorInDebugGodot.csproj";
    });
#else
    configuration = configuration.WithLingoGodotEngine(this);
#endif

    configuration
        .SetProjectFactory<MinimalDirectorGameFactoryGodot>()
        .BuildAndRunProject(sp => _serviceProvider = sp);
});
```

## Conditional project reference
```xml
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
  <ProjectReference Include="..\..\..\src\Director\LingoEngine.Director.LGodot\LingoEngine.Director.LGodot.csproj" />
</ItemGroup>
```

`MinimalDirectorGameFactoryGodot` keeps the project folder in the current directory so the relative project path works when the scene runs in the editor.

## Key files
- `Scenes/MinimalDirectorRoot.cs` configures the Lingo runtime and switches between Director and runtime-only modes via `#if DEBUG`.
- `MinimalDirectorGameFactoryGodot.cs` sets up project settings, cast members, and the startup movie.
- `MinimalDirectorGame.cs` stores the constants shared across the sample.

## Try it out
Open the folder in Godot 4.5 and run the scene in release mode to see the plain runtime.
Switch the build configuration to Debug to run again with the Director UI enabled.
