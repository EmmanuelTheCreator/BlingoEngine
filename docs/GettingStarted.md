# Getting Started

This short guide explains the layout of the repository and how to run the test suite locally.
For minimal end-to-end bootstraps, browse the [Sample Projects overview](../Samples/ReadMe.md).

## 📁 Project Structure

| Folder | Description |
|--------|-------------|
| `src/BlingoEngine` | Core Lingo runtime and engine abstractions |
| `src/BlingoEngine.LGodot` | Adapter for [Godot](https://godotengine.org/) |
| `src/BlingoEngine.SDL2` | Adapter for SDL2 |
| `src/Director` | Standalone Director application re‑implementation (basic movie, cast, and score features working) |
| `Demo/TetriGrounds` | Sample game showing usage with both backends |

🔎 For a detailed technical overview, see the [Architecture guide](design/Architecture.md).

## 🧪 Running Tests

This project uses the .NET SDK. You can run all unit tests with:

```bash
dotnet test
```

Need to install the SDK?

- Follow the [official install guide](https://learn.microsoft.com/dotnet/core/install/)
- Or run the helper script:

```bash
./scripts/install-dotnet.sh
```

For engine-specific setup, see the [Godot Setup](GodotSetup.md) and [SDL2 Setup](SDLSetup.md) guides.


