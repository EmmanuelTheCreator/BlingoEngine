# ðŸ—ï¸ BlingoEngine Architecture Overview

BlingoEngine is built as a **layered, modular system** that emulates the core behaviors of Macromedia Director using a modern C# runtime. Its architecture is designed to isolate core logic from platform-specific details â€” allowing you to reuse scripts and game logic across multiple rendering backends such as **Godot** and **SDL2**.

---

## ðŸ“ Architecture Layers

BlingoEngine is organized into four main architectural layers:
| Layer               | Description |
|---------------------|----------------------------------------------------------------------------------------------------------------------------------|
| **Core**            | The Lingo language runtime and virtual machine (VM) â€” fully rendering-agnostic. |
| **Framework Adapters** | Abstractions that allow the engine to run on multiple rendering platforms (Godot, SDL2, etc.). |
| **Director Layer**  | Optional high-level components that mimic Macromedia Director's original movie/cast/score model. |
| **Demo Projects**   | Sample integrations demonstrating how to use BlingoEngine with real frameworks and games. |
Each adapter implements a well-defined set of interfaces to ensure the **core engine remains untouched** regardless of the target platform.

---

## ðŸ”Œ Interfaces & Implementations

At the heart of the engine is the `src/BlingoEngine` project. It defines the key **engine interfaces** that abstract away rendering and platform specifics:

- `IBlingoFrameworkStage` â€“ represents the rendering surface
- `IBlingoFrameworkSprite` â€“ represents visual sprite elements
- `IBlingoFrameworkMovie` â€“ encapsulates timeline logic and score interaction
- `IBlingoFrameworkGfxNodeInput`, `IBlingoFrameworkMouse`, etc. â€“ abstract input handling
- `IBlingoFrameworkFactory` â€“ used to construct platform-native instances of all of the above

### Example: Framework Agnostic Usage

The core engine depends only on these interfaces:

```csharp
IBlingoFrameworkFactory factory = new GodotFactory(serviceProvider, root);
var stage = factory.CreateStage(new BlingoPlayer());
var movie = factory.AddMovie(stage, new BlingoMovie("Demo"));
```

This allows your game logic, Lingo scripts, and runtime behaviors to work **without ever knowing** if it's rendering with Godot or SDL2.

---

## ðŸ­ Factory Pattern

BlingoEngine uses the **Factory pattern** to inject framework-specific implementations.

- Each rendering adapter (e.g., Godot or SDL2) defines its own `IBlingoFrameworkFactory` implementation.
- These factories are usually registered in a **DI container** (e.g., `Microsoft.Extensions.DependencyInjection`).
- When the engine starts, it queries the factory to obtain the correct platform-native objects.

### Benefits:
- âœ… Decouples game logic from rendering details
- âœ… Allows easy addition of new adapters
- âœ… Encourages testability and interface-driven design

---

## ðŸ§ª Adapter Implementations

Each adapter project (e.g., `BlingoEngine.LGodot` or `BlingoEngine.SDL2`) provides concrete implementations for the core interfaces:

| Adapter              | Provided Classes |
|----------------------|--------------------------------------------------------------|
| `BlingoEngine.LGodot` | `GodotStage`, `GodotSprite`, `GodotMovie`, `GodotFactory`    |
| `BlingoEngine.SDL2`   | `SdlStage`, `SdlSprite`, `SdlMovie`, `SdlFactory`            |
These map to native objects in the respective frameworks while still adhering to the engine contracts.

---

## ðŸŽ¬ Optional Director Layer

The `src/Director` folder contains a **higher-level application layer** that mirrors Macromedia Director's built-in behaviors more closely:

- `BlingoEngine.Director.Movie` â€“ manages movie playback and score state
- `BlingoEngine.Director.Cast` â€“ emulates cast member access
- `BlingoEngine.Director.Stage` â€“ provides legacy stage behaviors
- `BlingoEngine.Director.Key/Sound/System/...` â€“ optional subsystems reflecting classic Director features

This layer is **optional** but useful for full-featured game recreation.

---

## ðŸ§ª Demo Projects

Demo implementations such as `Demo/TetriGrounds` showcase how to wire everything together:

- Uses `ServiceCollection` to register components
- Boots the game through the correct adapter (`WithBlingoSdlEngine()` or `WithBlingoGodotEngine()`)
- Demonstrates script-driven gameplay using real Lingo behaviors

---

## ðŸ“Œ Summary

BlingoEngine's architecture enables:

- ðŸ” **Script portability** between rendering platforms
- ðŸ” **Code clarity and separation of concerns**
- ðŸ§± **Scalable engine growth** through pluggable components
- ðŸ•¹ï¸ **Faithful Director emulation** with optional compatibility layers

---


## ðŸ§­ Architecture Diagram

```mermaid
graph TD
    demo["Demo Projects<br/>(TetriGrounds, etc.)"] --> director["Director Compatibility<br/>(Optional Application Layer)"]
    director --> core["Core Runtime<br/>Lingo VM + Engine Logic"]
    core -->|"Interfaces"| godot["Godot Adapter<br/>(LGodotFactory)"]
    core -->|"Interfaces"| sdl["SDL2 Adapter<br/>(SdlFactory)"]
    core -->|"Interfaces"| unity["Unity Adapter<br/>(UnityFactory)"]
    godot --> GodotStage
    godot --> GodotSprite
    godot --> GodotMovie
    sdl --> SdlStage
    sdl --> SdlSprite
    sdl --> SdlMovie
    unity --> UnityStage
    unity --> UnitySprite
    unity --> UnityMovie
```

### IOC Architecture Layers
```mermaid
graph TD
    start["Startup / Entry Point<br/>(e.g., Main, Godot.OnReady)"] --> config["ConfigureServices()<br/>Register BlingoEngine types"]
    config --> register["IBlingoFrameworkFactory is registered<br/>(GodotFactory / SdlFactory / Unity...)"]
    register --> resolve["Game code resolves services<br/>(e.g., BlingoPlayer, Stage)"]
```

This diagram illustrates how script handlers like onBeginSprite, onExitFrame, etc., are connected to the core VM logic and dispatched into sprite behaviors.
```mermaid
graph TD
    loader["Lingo Script Loader"] --> movie[BlingoMovie] --> score[BlingoScore] --> sprite[BlingoSprite] --> behavior["Sprite Behavior"]
    behavior --> begin[onBegin]
    behavior --> exit[onExitFrame]
    behavior --> extra[...]
```


### A flow schema:
```mermaid
graph TD
    start[Application Startup] --> reg["Register Services (DI)\n- IBlingoFrameworkFactory\n- IBlingoPlayer, Clock, ..."]
    reg --> resolve["Resolve IBlingoFrameworkFactory\n(GodotFactory / SdlFactory / UnityFactory)"]
    resolve --> godot[GodotFactory]
    resolve --> sdl[SdlFactory]
    resolve --> unity[UnityFactory]
    godot --> stageG[GodotStage]
    sdl --> stageS[SdlStage]
    unity --> stageU[UnityStage]
    stageG --> movie[BlingoMovie]
    stageS --> movie
    stageU --> movie
    movie --> score[BlingoScore]
    score --> alloc["Sprite Allocation -> IBlingoFrameworkSprite"]
    alloc --> behavior[BlingoSpriteBehavior Class]
    behavior --> bbegin[beginSprite]
    behavior --> bexit[exitFrame]
    behavior --> benter[enterFrame]
    behavior --> bmouse[mouseDown, etc]
```
### Summary
- From application startup, services and adapters are registered into a DI container.
- The core engine then resolves platform-specific types via IBlingoFrameworkFactory.
- BlingoMovie and BlingoScore manage the timeline and sprite orchestration.
- Sprite behaviors respond to dispatched events such as enterFrame, exitFrame, mouseDown, etc.



## ðŸ“Ž See Also

- [README.md](../../README.md)
- [Godot Setup Guide](../GodotSetup.md)
- [SDL2 Setup Guide](../SDLSetup.md)
- [Lingo vs C# Differences](Blingo_vs_CSharp.md)

---



