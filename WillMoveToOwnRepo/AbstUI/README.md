# AbstUI

AbstUI (Abstraction User Interface) is an abstraction layer that decouples user interface
components from specific rendering frameworks. It provides common UI primitives so the same
UI code can render consistently across SDL2, Godot, Unity, Blazor, and more. This allows
AbstUI to be reused independently of any particular engine.

## Project Structure

Source code lives under `src/` with a small project per backend:

- `AbstUI/` – core primitives and layout components.
- `AbstUI.SDL2/` – SDL2 renderer for the primitives.
- `AbstUI.LGodot/` – Godot integration layer.
- `AbstUI.LUnity/` – Unity integration layer.
- `AbstUI.Blazor/` – Blazor/WebAssembly backend.
- `AbstUI.ImGui/` – experimental ImGui renderer.
- `AbstUI.SDL2RmlUi/` – experimental SDL2 backend augmented with [RmlUi.NET](https://www.nuget.org/packages/RmlUi.NET) input widgets.

Visual test harnesses for the backends live in the `Test/` folder.

## Architecture

AbstUI defines a set of rendering‑agnostic controls (buttons, labels, layout containers, etc.).
Each backend translates these controls into native widgets for its target framework while
sharing the same high‑level component model. The separation enables the same UI definition to
run across multiple engines with minimal changes.

### Global Engine Flags

`AbstEngineGlobal` stores runtime flags that are shared across backends. The
static `RunFramework` value indicates which host (SDL2, Godot, Unity, etc.) is
currently driving the UI and allows components to branch on framework‑specific
behaviour. `IsRunningDirector` toggles extra features when the UI is embedded in
the Director‑style editor.

### Mouse Input

`AbstMouse` abstracts pointer state and exposes events such as `Moved` and
`ButtonPressed`. Backends implement `IAbstFrameworkMouse` to feed platform
events into the engine. Components can subscribe to `AbstMouseEventSubscription`
objects to react to clicks or hover state without depending on a particular
windowing system.

### Keyboard Input

Keyboard handling follows the same pattern: `AbstKey` represents individual
keys while `AbstKeyEvent` describes presses and releases. Framework adapters
implement `IAbstFrameworkKey` so text boxes and shortcut managers receive
notifications in a uniform way.

### Window Management

The `AbstWindowManager` coordinates top‑level windows and dialogs. A main window
creates additional `AbstWindow` instances for dialogs or context menus, and
each backend supplies an `IAbstFrameworkWindow` to handle native windowing
details. This keeps layout logic independent from the host platform.

### Command Handling

UI actions are decoupled from their effects through a lightweight command system. Components
create commands that are dispatched to registered handlers at runtime:

```csharp
public sealed record OpenWindowCommand(string WindowCode) : IAbstCommand;

public sealed class OpenWindowHandler : IAbstCommandHandler<OpenWindowCommand>
{
    public bool Handle(OpenWindowCommand cmd)
    {
        Console.WriteLine($"Opening {cmd.WindowCode}");
        return true;
    }
}

var manager = new AbstCommandManager(provider)
    .Register<OpenWindowHandler, OpenWindowCommand>();
manager.Handle(new OpenWindowCommand("settings"));
```

### Example: Button Component

Controls such as `AbstButton` expose a framework‑agnostic API while backends
provide the platform implementation through `IAbstFrameworkButton`:

```csharp
// Resolve the component factory from dependency injection
var factory = serviceProvider.GetRequiredService<IAbstComponentFactory>();
var button = factory.CreateButton("play", "Play");
button.Pressed += () => Console.WriteLine("Pressed!");

// SDL2 backend implementation
internal class AbstSdlButton : AbstSdlComponent, IAbstFrameworkButton
{
    public event Action? Pressed;
    // framework-specific rendering omitted
}

// Interface each backend implements
public interface IAbstFrameworkButton : IAbstFrameworkNode
{
    event Action? Pressed;
}
```
