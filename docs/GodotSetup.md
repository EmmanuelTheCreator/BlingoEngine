# Setting Up the Godot Runtime

The Godot adapter embeds BlingoEngine into a Godot scene through a framework
*factory*. Review [ProjectSetup](ProjectSetup.md) for the general registration
flow, then apply the steps below for a Godot project:

1. Install **Godot 4** from the [official website](https://godotengine.org/).
2. Open `BlingoEngine.Demo.TetriGrounds.Godot.sln` with your C# IDE or open the
   `project.godot` file directly in Godot.
3. Ensure the Godot C# tools are configured. In the Godot editor, open
   **Project â†’ Tools â†’ C#** and set the path to `dotnet` if required.
4. Register the Godot factory in your root node and run the project:

```csharp
using BlingoEngine.LGodot;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

public partial class RootNodeTetriGrounds : Node2D
{
    private readonly ServiceCollection _services = new();

    public override void _Ready()
    {
        _services.RegisterBlingoEngine(cfg => cfg
            .WithBlingoGodotEngine(this, factory =>
            {
                // optional Godot factory configuration
            })
            .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
            .BuildAndRunProject());
    }
}
```

The factory supplies Godotâ€‘specific implementations for graphics, input and
windowing before the project starts.

## Director

The `BlingoEngine.Director.LGodot` package exposes a classic Director-like
authoring environment. Register it instead of the plain factory when you want
the full windowed interface:

```csharp
_services.RegisterBlingoEngine(cfg => cfg
    .WithDirectorGodotEngine(this)
    .SetProjectFactory<MyProjectFactory>()
    .BuildAndRunProject());
```

Stage dimensions define the playable stage area and are set in your
`IBlingoProjectFactory` implementation:

```csharp
public void Setup(IBlingoEngineRegistration config)
{
    config.WithProjectSettings(s =>
    {
        s.StageWidth = 640;
        s.StageHeight = 480;
    });
}
```

In Godot, set **Project Settings â†’ Display â†’ Window â†’ Width/Height** to values larger than the stage (for example, 1280Ã—960) so the Director window has room for the interface.

For more details on how the factory wires everything together, see
[ProjectSetup](ProjectSetup.md).


