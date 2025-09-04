# Project Setup

LingoEngine runs on multiple frameworks by delegating platform specific work to
an `ILingoFrameworkFactory`.  The factory provides rendering, input, sound and
other services for the engine.  Use `RegisterLingoEngine` to configure the
engine and select the factory for your target framework.

## Basic registration

```csharp
using LingoEngine.SDL2; // or LingoEngine.LGodot
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.RegisterLingoEngine(cfg => cfg
    // Choose one of the available framework factories
    .WithLingoSdlEngine("MyGame", 1280, 960)       // SDL2 Director window
    // .WithLingoGodotEngine(this)                // Godot scene
    .SetProjectFactory<MyGameProjectFactory>()     // loads casts and movies
    .BuildAndRunProject());

var provider = services.BuildServiceProvider();
// For SDL2, start the loop:
provider.GetRequiredService<SdlRootContext>().Run();
```
`WithLingoSdlEngine(title, width, height)` defines the Director window size; ensure these values exceed the stage dimensions.

`SetProjectFactory<T>()` registers your `ILingoProjectFactory` implementation
which loads cast libraries and the startup movie.  `BuildAndRunProject()` wires
everything together and prepares the engine to run.

## Sample `ILingoProjectFactory`

This factory wires fonts, a 640×480 stage, movie scripts, and custom services before the engine starts.

```csharp
using LingoEngine.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

public class MyGameProjectFactory : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration config)
    {
        config
            .AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
            .AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"))
            .WithProjectSettings(s =>
            {
                s.ProjectFolder = "TetriGrounds";
                s.ProjectName = "TetriGrounds";
                s.MaxSpriteChannelCount = 300;
                s.StageWidth = 640;
                s.StageHeight = 480;
            })
            .ForMovie("Intro", s => s
                .AddScriptsFromAssembly()

                // As an example, you can add them manually too:

                // .AddMovieScript<StartMoviesScript>()    // Movie script
                // .AddBehavior<MouseDownNavigateBehavior>() // Behavior
                // .AddParentScript<BlockParent>()     // Parent script
            )
            .ServicesLingo(s => s
                .AddSingleton<ITetriGroundsCore, TetriGroundsCore>()
                .AddSingleton<GlobalVars>()
            );
    }

    public void LoadCastLibs(ILingoCastLibsContainer castlibs, LingoPlayer player)
        => player.LoadCastLibFromCsv("Data", Path.Combine("Media", "Data", "Members.csv"));

    public ILingoMovie? LoadStartupMovie(ILingoServiceProvider services, LingoPlayer player)
        => player.NewMovie("Intro");

    public void Run(ILingoMovie movie, bool autoPlayMovie)
    {
        if (autoPlayMovie) movie.Play();
    }
}
```

This implementation defines four required methods:
- `Setup(ILingoEngineRegistration config)` registers fonts, project settings (including the 640×480 stage), movies, and services.
- `LoadCastLibs(ILingoCastLibsContainer castlibs, LingoPlayer player)` loads external cast libraries using the provided player.
- `LoadStartupMovie(ILingoServiceProvider services, LingoPlayer player)` chooses the movie that starts first.
- `Run(ILingoMovie movie, bool autoPlayMovie)` finalizes startup and optionally plays the movie automatically.

### Set properties of members

`InitMembers` receives the running `LingoPlayer` so you can fetch cast libraries and adjust members.

```csharp
public void InitMembers(LingoPlayer player)
{
    var textColor = LingoColor.FromHex("#999966");
    var text = player.CastLib(2).GetMember<LingoMemberText>("T_data");
    text!.TextColor = textColor;
}
```

### Add labels

`SetScoreLabel(frame, label)` tags a frame number with a label string for navigation.

```csharp
public void AddLabels()
{
    _movie.SetScoreLabel(2, "Intro");
    _movie.SetScoreLabel(60, "Game");
    _movie.SetScoreLabel(75, "FilmLoop Test");
}
```

### Adding sprites

`AddSprite(channel, beginFrame, endFrame, x, y, configure)` creates a sprite on a channel for the frame range and positions it at `(x, y)`. The optional `configure` action lets you tweak the sprite immediately after creation.

```csharp
_movie.AddSprite(9, 60, 64, 519, 343, sprite =>
{
    sprite.SetMember("B_Play")
          .AddBehavior<ButtonStartGameBehavior>();
});
```

### Add a frame script

`AddFrameBehavior<T>(frame)` attaches behavior `T` to the specified frame number.

```csharp
_movie.AddFrameBehavior<GameStopBehavior>(60);
```

## Director

To enable the optional Director interface, use the dedicated registration
method (currently Godot only):

```csharp
services.RegisterLingoEngine(cfg => cfg
    .WithDirectorGodotEngine(rootNode)
    .SetProjectFactory<MyGameProjectFactory>()
    .BuildAndRunProject());
```

The stage width and height configured in `Setup` define the playable stage area.
The Director window should be larger and is configured via the runtime (for SDL2 through `WithLingoSdlEngine` and for Godot via project settings).
All examples here use a 640×480 stage.

Each framework exposes its own factory type (`SdlFactory`, `GodotFactory`, …).
You can pass a callback to the `WithLingo*Engine` method to tweak the factory
before the engine starts.

### Conditional Director usage

You can enable the Director interface only in debug builds by defining a
compile‑time constant in your project file:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);WITH_DIRECTOR</DefineConstants>
</PropertyGroup>
```

Then switch registration based on that constant:

```csharp
#if WITH_DIRECTOR
services.RegisterLingoEngine(cfg => cfg
    .WithDirectorGodotEngine(rootNode)
    .SetProjectFactory<MyGameProjectFactory>()
    .BuildAndRunProject());
#else
services.RegisterLingoEngine(cfg => cfg
    .WithLingoGodotEngine(rootNode)
    .SetProjectFactory<MyGameProjectFactory>()
    .BuildAndRunProject());
#endif
```

See [SDLSetup](SDLSetup.md) and [GodotSetup](GodotSetup.md) for framework
specific notes.

