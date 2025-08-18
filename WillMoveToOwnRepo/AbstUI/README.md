# AbstUI

AbstUI (Abstraction User Interface) is an abstraction layer that decouples user interface components from specific rendering frameworks. It provides common UI primitives so the same UI code can render consistently across SDL2, Godot, Unity, or other platforms. This allows AbstUI to be reused independently of any particular engine.

The repository currently includes the core abstractions and optional backends:
- `AbstUI`: core primitives and components.
- `AbstUI.SDL2`: SDL2 backend.
- `AbstUI.LGodot`: Godot backend.
- `AbstUI.LUnity`: Unity backend.
- `AbstUI.Blazor`: Blazor backend.
