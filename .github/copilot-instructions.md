# BlingoEngine - GitHub Copilot Instructions

**Always reference these instructions first** and fallback to search or bash commands only when you encounter unexpected information that does not match the guidance here.

BlingoEngine is a modern, cross-platform C# runtime that emulates Macromedia Director's Lingo scripting language. It enables playback of original Lingo code on modern rendering backends like Godot, SDL2, Unity, and Blazor.

## Working Effectively

### Bootstrap and Environment Setup
- **CRITICAL**: Do not work on `main` branch. Always work on feature branches.
- Install all required dependencies:
  ```bash
  ./scripts/install-packages-linux.sh
  ```
  - **NEVER CANCEL**: Setup takes 2-3 minutes to download .NET SDKs and Godot. Set timeout to 5+ minutes.
  - This installs:
    - .NET 8 LTS (required for Godot integration)
    - .NET 9 (required for Blazor projects)
    - SDL2 development libraries and X11/GL dependencies
    - Godot 4.5 mono editor and runtime
    - `dotnet-format` global tool

- **Always ensure PATH includes**: `$HOME/.dotnet:$HOME/.dotnet/tools:$HOME/.local/bin`
  ```bash
  export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$HOME/.local/bin:$PATH"
  ```

### Build Commands
- **Build specific project** (recommended):
  ```bash
  dotnet build Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj
  ```
  - **Timing**: Takes ~3.5 seconds for TetriGrounds SDL2 demo
  - **Timing**: Takes ~5.5 seconds for TetriGrounds Blazor demo

- **Build entire solution** (use sparingly):
  ```bash
  dotnet build BlingoEngine.sln
  ```
  - **NEVER CANCEL**: Full solution build takes 60+ seconds and may have expected errors in Unity integration. Set timeout to 90+ minutes.
  - **Known Issues**: Some Unity tests fail (expected), some Director file paths missing (expected)

### Test Execution
- **ALWAYS run tests only for affected projects**, never the entire solution.

- **Core Lingo engine tests**:
  ```bash
  dotnet test Test/BlingoEngine.Lingo.Core.Tests/BlingoEngine.Lingo.Core.Tests.csproj
  ```
  - **Timing**: Takes ~10 seconds with 126 tests (1 may be skipped - this is normal)
  - **NEVER CANCEL**: Set timeout to 15+ minutes

- **Additional BlingoEngine tests**:
  ```bash
  dotnet test Test/BlingoEngine.Tests/BlingoEngine.Tests.csproj
  ```
  - **Timing**: Takes ~3.4 seconds with 29 tests

- **ProjectorRays tests**:
  ```bash
  dotnet test WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj
  ```

- **Visual test projects** (not unit tests):
  - `Test/BlingoEngine.SDL2.GfxVisualTest/BlingoEngine.SDL2.GfxVisualTest.csproj` - console app for manual SDL2 testing
  - `WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.*` - manual console apps
  - Use `dotnet run` or `dotnet build`, NOT `dotnet test`

### Validation
- **ALWAYS manually validate changes** by running affected demos:
  ```bash
  dotnet run --project Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj
  ```
  - **Expected behavior**: Builds and starts successfully but may fail on missing fonts/audio in headless environment
  - **Validation success**: If it builds, starts, and shows initialization messages before failing on missing resources

- **Test the Blazor demo**:
  ```bash
  dotnet run --project Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Blazor/BlingoEngine.Demo.TetriGrounds.Blazor.csproj
  ```

- **Always run linting before committing**:
  ```bash
  dotnet format <path/to/project.csproj> --include <relative/path/to/file.cs> -v diagnostic
  ```

## Project Structure and Navigation

### Key Projects
| Project Path | Purpose | Build Target |
|--------------|---------|--------------|
| `src/BlingoEngine/` | Core engine and dependency injection | .NET 8 |
| `src/BlingoEngine.Lingo.Core/` | Lingo scripting runtime | .NET 8 |
| `src/BlingoEngine.SDL2/` | SDL2 rendering backend | .NET 8 |
| `src/BlingoEngine.LGodot/` | Godot integration layer | .NET 8 |
| `src/BlingoEngine.Blazor/` | Blazor web integration | .NET 9 |
| `Demo/TetriGrounds/` | Reference game implementation | Multiple backends |
| `src/Director/` | Director authoring environment | .NET 8 |

### Important Files
- `BlingoEngine.sln` - Main solution file
- `global.json` - Specifies .NET 9 SDK requirement
- `AGENTS.md` - Agent-specific development instructions
- `setup-linux.sh` / `setup-windows.bat` - Platform setup scripts

### Frequently Modified Areas
- **Core engine changes**: Always test with `Test/BlingoEngine.Lingo.Core.Tests/`
- **SDL2 changes**: Build and run TetriGrounds SDL2 demo
- **Blazor changes**: Build and run TetriGrounds Blazor demo
- **Cast/Movie logic**: Found in `Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Core/`

## Common Tasks

### Adding New Features
1. **Always create feature branch** (not on main)
2. **Follow existing architecture patterns** - see TetriGrounds demo for reference
3. **Update relevant tests** in corresponding test project
4. **Build and validate** affected demos work
5. **Run dotnet format** on modified files

### Code Style and Standards
- **Use `dotnet format`** for style issues - never manually format
- **Search with `rg`** (ripgrep) instead of `grep` when available
- **Do not remove existing comments** from code
- **Class member order**: fields, properties, constructors
- **Avoid business logic in interfaces** (maintains .NET Framework 4.8 compatibility)

### Debugging Issues
- **Check AGENTS.md first** for project-specific guidance
- **Build specific projects** to isolate issues
- **Look for missing font/media files** - often causes runtime failures in demos
- **Unity integration issues** are expected and can be ignored unless working on Unity specifically

### Working with Backends
- **SDL2**: Lightweight desktop windows, good for testing
- **Godot**: Full game engine integration, requires Godot installed
- **Blazor**: Web applications, uses .NET 9
- **Unity**: Limited integration, many tests expected to fail

## Troubleshooting

### Common Build Issues
- **"Framework not found" errors**: Run `./scripts/install-packages-linux.sh`
- **Missing Godot references**: Set `GODOT_URL` environment variable or use setup script
- **Unity test failures**: Expected - ignore unless specifically working on Unity integration
- **Font/media file errors**: Expected in headless environments - indicates app is starting correctly

### Performance Expectations
- **Individual project builds**: 3-6 seconds
- **Full solution build**: 60+ seconds (has expected failures)
- **Core tests**: 10 seconds, 126 tests pass
- **Additional tests**: 3-4 seconds, 29 tests pass
- **Setup script**: 2-3 minutes for full environment

### Known Working Configurations
- **TetriGrounds SDL2**: Builds, starts, validates basic engine functionality
- **TetriGrounds Blazor**: Builds, demonstrates web integration
- **Core test suite**: Comprehensive validation of Lingo engine
- **Linting**: dotnet-format tool works correctly

Remember: **BlingoEngine bridges legacy Director/Lingo content with modern engines**. Always validate that changes preserve compatibility with original Lingo semantics while enabling modern deployment targets.
