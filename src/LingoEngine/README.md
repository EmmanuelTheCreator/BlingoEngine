# LingoEngine Core

The core engine library hosts the runtime that executes Lingo scripts and exposes high‑level
abstractions that are shared by every backend. The structure mirrors classic Macromedia
Director concepts so that movies, casts, and sprites behave the same regardless of the
rendering platform.

## Folder Layout

- `Core/` – shared utilities and dependency‑injection helpers.
- `FrameworkCommunication/` – interfaces used by platform adapters to talk to the engine.
- `Animations/`, `Sprites/`, `Bitmaps/`, `Sounds/`, `Movies/`… – implementations of common
  Director building blocks.
- `Projects/` – project definitions and bootstrap logic.
- `Setup/` and `LingoEngineSetup.cs` – extension methods to wire everything up in a service
  collection.
- `Tools/`, `Scripts/`, and other folders provide supporting utilities, assets, or build
  helpers.

The engine is designed around dependency injection so that host applications can register only
the services they need. Each subfolder represents a small, focused feature area that can be
extended without touching the rest of the codebase.

## Animations

`Animations/` contains tweening helpers and sprite animators. Key frames describe how a value
changes over time:

```csharp
var tween = new LingoTween<int>();
tween.AddKeyFrame(1, 0);
tween.AddKeyFrame(30, 100);
```

## Sprites

Sprites are the visual actors on stage. Movies create and configure them via `AddSprite`:

```csharp
var sprite = movie.AddSprite(1, s => s.LocH = 100);
```

## Bitmaps

Bitmap members wrap image data for sprites to display:

```csharp
var cast = movie.CastLibsContainer.ActiveCast;
var bitmap = cast.Add<LingoMemberBitmap>(1, "Logo");
```

## Sounds

The sound system manages channels and playback:

```csharp
var channel = movie.Sound.GetSoundChannel(1);
channel.Play(memberSound);
```

## Movies

Movies orchestrate sprites, sounds, and scripts. A movie can be created and played through the
player API:

```csharp
var movie = lingoPlayer.NewMovie("Intro");
movie.Play();
```

## Projects

Projects describe how a game is wired together. Implement `ILingoProjectFactory` to configure
services, load cast libraries, and start the initial movie:

```csharp
public class MyProjectFactory : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration config)
        => config.WithProjectSettings(s => s.ProjectName = "MyProject");
}
```

### Example: TetriGrounds

The TetriGrounds demo wires these pieces together using the SDL2 backend:

```csharp
services.RegisterLingoEngine(c => c
    .WithLingoSdlEngine("TetriGrounds", 730, 547)
    .SetProjectFactory<TetriGroundsProjectFactory>()
    .BuildAndRunProject());
```
