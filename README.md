# BlingoEngine



**BlingoEngine** is a modern, cross-platform C# runtime designed to emulate Macromedia Director's **Lingo** scripting language. It enables playback of original Lingo code and behaviors on top of modern rendering backends like **Godot**, **SDL2**, **Unity**, and **Blazor**, allowing legacy projects to be revived or reimagined with full flexibility.

<img src="Images/logo_godot.png" alt="Godot" width="20%" /><img src="Images/logo_SDL.png" alt="SDL" width="20%" /><img src="Images/logo_blazor.png" alt="Blazor" width="7%" /><img src="Images/logo_Unity.jpg" alt="Unity" width="7%" />



---

### Director.NET

![Director full screenshot](Images/Direcor-FullScreenshot.jpg)

#### Property inspector

<img src="Images/PropertyInspector.png" alt="Property Inspector" width="49%" /><img src="Images/TempoChange.jpg" alt="Tempo Change" width="49%" />

#### Easy Lingo to C# conversion
File by file or in batch with the lingo importer.

<img src="Images/Director_CodeConverter1.jpg" alt="Easy Lingo to C# conversion" width="100%" />


#### Remote Terminal
An easy way to debug your game, run the Remote Terminal trough pipes or SignalR.

<img src="Images/Screenshot_RNetTerminal1.jpg" alt="RNetTerminal1" width="30%" /><img src="Images/Screenshot_RNetTerminal2.jpg" alt="RNetTerminal2" width="30%" /><img src="Images/Screenshot_RNetTerminal3.jpg" alt="RNetTerminal3" width="30%" />


## âœ¨ Key Features of the engine

- âœ… **Lingo Script Execution** â€“ Runs legacy Macromedia Director scripts directly in C#.
- ðŸ”Œ **Pluggable Rendering Backends** â€“ Clean architecture supporting:
  - [Godot Engine](https://godotengine.org/)
  - [SDL2](https://www.libsdl.org/)
  - [Unity](https://unity.com/)
  - [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- ðŸ§  **Director application** â€“ Offers basic movie, cast, and score compatibility and can run standalone or as a library in your project.
- ðŸ§© **Modular Runtime Architecture** â€“ Clear separation of concerns: input, rendering, audio, system services, and script execution.
- âš™ï¸ **Service-Oriented Initialization** â€“ Uses dependency injection and service collections for clean setup.
- ðŸŒ **Cross-Platform Compatibility** â€“ Works anywhere the .NET SDK is available.

---

## Help making this project!


> âš ï¸ **Can you help us make this dream project come true?**
> This project is still under heavy development, and we can use some help. Reach out if you want to contribute.
> ðŸš§ **Warning:** The Director SDL integration is still under heavy development and is not yet functional.
.
.

---

## ðŸŽ‰ Standing on the Shoulders of Giants

**Macromedia Director** and its **Lingo language** were revolutionary in their time, like [John Henry Thompson ](https://johnhenrythompson.com/johnhenrythompson/) and [Marc Canter](https://en.wikipedia.org/wiki/Marc_Canter) who created them.
They empowered an entire generation of artists, educators, and game developers to create interactive experiences long before todayâ€™s engines existed.

Director pioneered ideas that shaped the future of digital creativity:  
- The **stage, cast, and score** metaphor made multimedia authoring approachable  
- The **Lingo scripting language** gave non-programmers the power to create interactivity  
- A vibrant global community pushed the boundaries of art, education, and entertainment  

**BlingoEngine** is not here to replace Director, but to *honor its spirit* â€” carrying those ideas forward into the modern era so they can continue to inspire.  

| Directorâ€™s Legacy âœ¨            | BlingoEngineâ€™s Contribution ðŸš€ |
|--------------------------------|--------------------------------|
| First accessible multimedia authoring tool for creatives | Keeps Lingo projects alive on modern platforms |
| Introduced the stage, cast, score, and Lingo scripting concepts | Brings those concepts into C# and todayâ€™s engines |
| Enabled art, education, and indie game communities worldwide | Opens them again for exploration, study, and reuse |
| Inspired countless developers and later tools (Flash, Unity, etc.) | Bridges history with modern ecosystems like Godot, SDL2, Unity, Blazor |

> ðŸ§¡ To the Director developers and community:  
> we applaud your achievements, and BlingoEngine exists thanks to the foundation you built.


---

## â­ Why Use BlingoEngine?

- ðŸš€ Port legacy Director projects to modern engines  
- ðŸ” Reuse existing assets, scripts, and logic  
- ðŸ› ï¸ Build hybrid projects that combine old logic with new rendering  
- ðŸ•¹ï¸ Explore the inner workings of Director games using readable C# code  
- ðŸ’¾ Preserve interactive media history with a modern toolset  


---
## The Lingo Verbose Language
Looking for a more expressive C# syntax? The `BlingoEngine.VerboseLanguage` package offers a fluent API that mirrors classic Lingo statements.


```csharp
// Lingo : put the Text of member "Paul Robeson" into member "How Deep"

// C# with BlingoEngine.VerboseLanguage
Put(The().Text.Of.Member("Paul Robeson")).Into.Field("How Deep");
```

## ðŸš€ Running the Demo

1. **Clone the repository**:

   ```bash
   git clone https://github.com/EmmanuelTheCreator/BlingoEngine.git
   cd BlingoEngine
   ```

2. **Run installer with prerequisites**
   Ensure the .NET 8 SDK is available. You can install it using the helper script:

Linux:
   ```bash
   ./setup-linux.sh
   ```
Windows:
   ```bash
   ./setup-windows.sh
   ```

3. **Open the solution**
   Open `BlingoEngine.sln` in your preferred C# IDE (Visual Studio / Rider).

4. **Build a demo**
   Navigate to `Demo/TetriGrounds` and run one of the included platform integrations.

ðŸ‘‰ Use the dedicated guides for full setup instructions:

- [Godot Setup](docs/GodotSetup.md)
- [SDL2 Setup](docs/SDLSetup.md)
- [Blazor Demo](docs/BlazorDemo.md)
- [Sample Projects](Samples/ReadMe.md)

### VS Code Setup

1. Install the [.NET SDK](https://learn.microsoft.com/dotnet/core/install/) and [Godot 4.5](https://godotengine.org/) with C# support.
2. Open the repository folder in VS Code and accept the recommended extensions.
3. Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>B</kbd> to build the solution.
4. From the Run and Debug panel choose **Launch Demo SDL2** or **Launch Demo Godot**.


---

## ðŸŽ® Getting Started with Development

Need a concrete reference? Check the [Sample Projects overview](Samples/ReadMe.md) for minimal SDL2 and Godot setups.

Both the SDL2 and Godot frontends share the same backend logic. Here's an example of how to bootstrap the SDL2 engine:

```csharp
var services = new ServiceCollection();
services.RegisterBlingoEngine(cfg => cfg
    .WithBlingoSdlEngine("TetriGrounds", 1280, 960)
    .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
    .BuildAndRunProject());

var provider = services.BuildServiceProvider();
provider.GetRequiredService<SdlRootContext>().Run();
```
The window dimensions above create a Director window larger than the 640Ã—480 stage configured in the project factory.

Swap to the Godot backend by using `.WithBlingoGodotEngine(...)`.

ðŸ“„ See the [Getting Started guide](docs/GettingStarted.md), [Godot Setup](docs/GodotSetup.md), [SDL2 Setup](docs/SDLSetup.md), and [Blazor Demo](docs/BlazorDemo.md) for exact details.

---

## ðŸ“š Documentation

### Guides

- [Getting Started](docs/GettingStarted.md)
- [Lingo vs C# Differences](docs/design/Blingo_vs_CSharp.md)
- [Architecture Overview](docs/design/Architecture.md)
- [Godot Setup](docs/GodotSetup.md)
- [SDL2 Setup](docs/SDLSetup.md)
- [Blazor Demo](docs/BlazorDemo.md)
- [Project Setup](docs/design/ProjectSetup.md)
- [Progress Log](docs/Progress.md)
- [Director Keyframe Tags](docs/DirDissasembly/director_keyframe_tags.md)
- [Director Lingo MX2004 Scripting Guide](docs/Director_Blingo_mx2004_scripting.pdf)
- [XMED File Comparisons](docs/DirDissasembly/XMED_FileComparisons.md)
- [XMED Offsets](docs/DirDissasembly/XMED_Offsets.md)
- [Text Styling Example](docs/DirDissasembly/Text_Multi_Line_Multi_Style.md)

### API Reference

Documentation generated from the source code is available using [DocFX](https://github.com/dotnet/docfx). Run `scripts/build-docs.sh` (or `scripts/build-docs.ps1` on Windows) to produce the site in `docs/docfx/_site`. The pages include "View Source" links back to the repository.

---

## ðŸ§­ Roadmap

### ðŸŸ£ BlingoEngine Runtime â–“â–“â–“â–“â–“â–“â–“â–‘ 70%
The core runtime that executes Lingo scripts and connects to backends.

#### Core
| Feature                          | Status / Progress |
|----------------------------------|-------------------|
| Lingo Script Execution           | âœ… Stable |
| Lingo â†’ C# Conversion            | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–Œâ–‘â–‘ 75% |
| Lingo bytecode (dcode) interpreter | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ Experimental |

#### Backends
| Backend                          | Status / Progress |
|----------------------------------|-------------------|
| Godot Backend                    | âœ… Tested, working |
| SDL2 Backend                     | âœ… Tested, working |
| Unity Backend                    | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘ 70% (written, not fully tested) |
| Blazor Backend                   | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘ 70% (written, not fully tested) |

#### Features
| Feature                          | Status / Progress |
|----------------------------------|-------------------|
| FilmLoops                        | âœ… Done |
| Transitions                      | âœ… Done |
| Audio Playback                   | âœ… Done |
| Sprites2D                        | âœ… Done |
| Video Playback                   | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘ 90% |
| Macromedia Flash Integration     | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ Far future (0%) |
| BlingoEngine 3D                   | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ Early idea (2%) |

---

### ðŸŸ  Director Application â–“â–“â–“â–‘â–‘â–‘â–‘â–‘ 35%
A modern reimplementation of Directorâ€™s movie, cast, and score system on top of the runtime.

#### Backends
| Backend                          | Status / Progress |
|----------------------------------|-------------------|
| Godot Frontend                   | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘ 65% |
| SDL2 Frontend                    | â–ˆâ–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 15% |
| Unity Frontend                   | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ Planned |
| Blazor Frontend                  | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ Planned |

#### Core Systems
| Feature                          | Status / Progress |
|----------------------------------|-------------------|
| Score                            | âœ… Done |
| Cast                             | âœ… Done |
| Tempo                            | âœ… Done |
| Property Inspector               | âœ… Done |
| Text Editing                     | â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘â–‘ 70% |
| Picture Painter (Godot)          | â–ˆâ–ˆâ–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 30% (ðŸŽ¨ experimental / fun) |
| Shape Painter                    | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 0% (todo) |
| Color Palettes                   | â–ˆâ–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 5% |
| Orion Skin                       | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 0% (planned) |
| Behavior Code Library            | â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘â–‘ 0% (planned) |
| .dir File Importer               | â–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘â–‘â–‘ 50% |

---

âœ… = ready and tested  
â³ = in progress  
ðŸ§ª = experimental  
ðŸŽ¨ = playful / for fun  


---

## ðŸ¤ Contributing

We welcome contributions from the community!

To get started:

1. Fork this repository
2. Create a feature branch
3. Write your code and tests
4. Submit a pull request

Please include examples or documentation when appropriate.

Please also read our [Code of Conduct](CODE_OF_CONDUCT.md).

---

## Architecture overview.

```mermaid
graph TD

%% Top-level flow
A[Your game] --> B[BlingoEngine Runtime Core]
B --> C[Services]
B --> D[Rendering Abstraction Layer]
D --> E1[Godot]
D --> E2[SDL2]
D --> E3[Unity]
D --> E4[Blazor]

```

---

## ðŸ“„ License

Licensed under the [MIT License](LICENSE).

> **Note:** The TetriGrounds demo's assets are not covered by the MIT License. See [Demo/TetriGrounds/LICENSE.assets.txt](Demo/TetriGrounds/LICENSE.assets.txt) for details.

---

## ðŸ™‹â€â™‚ï¸ Questions or Feedback?

Feel free to [open an issue](https://github.com/EmmanuelTheCreator/BlingoEngine/issues) or start a discussion. We're happy to help, and open to ideas!


