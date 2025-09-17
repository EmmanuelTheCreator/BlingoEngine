# Setting Up the SDL2 Runtime

Use the SDL2 front‑end when you want a lightweight desktop window without a
full game engine. The SDL adapter is configured through the framework
*factory* that BlingoEngine uses to communicate with the host environment.

For a walk‑through of the common registration steps, see
[ProjectSetup](ProjectSetup.md). The snippet below shows the SDL‑specific
part:

1. Install the **SDL2** development libraries for your operating system.
2. Open `BlingoEngine.Demo.TetriGrounds.SDL2.csproj` with your favorite C# IDE.
3. Restore the NuGet packages so the SDL2‑CS bindings are available.
4. Register the SDL factory and run the demo:

```csharp
using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.RegisterBlingoEngine(cfg => cfg
    .WithBlingoSdlEngine("TetriGrounds", 640, 480, factory =>
    {
        // optional SDL factory configuration
    })
    .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
    .BuildAndRunProject());

var provider = services.BuildServiceProvider();
provider.GetRequiredService<SdlRootContext>().Run();
```
`WithBlingoSdlEngine(title, width, height)` defines the Director window size. This sample uses 640×480 to match the stage, but you may choose larger dimensions as long as they are at least as large as the stage configured in your project factory.

The factory creates framework‑specific services (rendering, input, sound…) and
is the place to configure SDL options before the engine starts.

## Director

An SDL-based Director interface is not yet available. Use the Godot adapter if
you need the full Director authoring environment.

For a complete overview of the registration process, see
[ProjectSetup](ProjectSetup.md).

