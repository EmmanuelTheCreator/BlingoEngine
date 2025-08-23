# AGENTS

These instructions apply to the entire repository.

## Environment
- The project targets **.NET 8**. If the `dotnet` CLI isn't available, install it with `./scripts/install-dotnet.sh` and ensure `$HOME/.dotnet` is on your `PATH`.

## Testing
- Run tests only for the projects affected by your changes; do not run the entire solution.
- For changes in core engine code (e.g., under `src` or `Test/LingoEngine.Lingo.Core.Tests`), run `dotnet test Test/LingoEngine.Lingo.Core.Tests/LingoEngine.Lingo.Core.Tests.csproj`.
- For changes in the ProjectorRays area (`WillMoveToOwnRepo/ProjectorRays`), run `dotnet test WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj`.
- Apply the same approach for other components: run `dotnet test <path-to-test-project>` for each modified project.
- Project `LingoEngine.SDL2.GfxVisualTest.csproj` is a console application to test the UI visually, not with tests in it.
 - Other visual test projects such as `WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.*` are also manual console apps; build or run them with `dotnet build`/`dotnet run` rather than `dotnet test`.

## Code Style
- Use `dotnet format` to fix style issues when needed.
- Prefer `rg` (ripgrep) over `grep` for searching the codebase.
- Do not remove existing comments from code.
- When writing new classes, place members in the order: fields, then properties, then constructors.
- Avoid adding business logic or default implementations inside interfaces to preserve .NET Framework 4.8 compatibility.

## Project Structure

| Path | Description |
| --- | --- |
| src/LingoEngine/LingoEngine.csproj | Core engine functionality and dependency injection setup |
| src/LingoEngine.Lingo.Core/LingoEngine.Lingo.Core.csproj | Runtime implementation of the Lingo scripting language |
| src/LingoEngine.IO/LingoEngine.IO.csproj | File and resource I/O built on LingoEngine |
| src/LingoEngine.IO.Data/LingoEngine.IO.Data.csproj | Shared data structures for the I/O layer |
| src/LingoEngine.SDL2/LingoEngine.SDL2.csproj | SDL2 bindings and rendering support |
| src/LingoEngine.Unity/LingoEngine.Unity.csproj | Unity engine integration layer |
| src/LingoEngine.LGodot/LingoEngine.LGodot.csproj | Godot engine integration layer |
| src/LingoEngine.3D.Core/LingoEngine.3D.Core.csproj | Core components for 3D features |
| src/Director/LingoEngine.Director.Core/LingoEngine.Director.Core.csproj | Editor tooling reminiscent of Macromedia Director |
| src/Director/LingoEngine.Director.LGodot/LingoEngine.Director.LGodot.csproj | Godot integration for Director tooling |
| Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.Core/LingoEngine.Demo.TetriGrounds.Core.csproj | Shared code for the TetriGrounds demo |
| Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.SDL2/LingoEngine.Demo.TetriGrounds.SDL2.csproj | SDL2-based TetriGrounds demo |
| Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.Godot/LingoEngine.Demo.TetriGrounds.Godot.csproj | Godot-based TetriGrounds demo |
| Test/LingoEngine.Lingo.Core.Tests/LingoEngine.Lingo.Core.Tests.csproj | Tests for the Lingo scripting runtime |
| Test/LingoEngine.Lingo.Tests/LingoEngine.Lingo.Tests.csproj | Tests for LingoEngine core features |
| Test/LingoEngine.SDL2.GfxVisualTest/LingoEngine.SDL2.GfxVisualTest.csproj | Console app for manual SDL2 graphics checks |
| WillMoveToOwnRepo/AbstUI/src/AbstUI/AbstUI.csproj | Core abstractions for the AbstUI framework |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.SDL2/AbstUI.SDL2.csproj | SDL2 backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.LUnity/AbstUI.LUnity.csproj | Unity backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.LGodot/AbstUI.LGodot.csproj | Godot backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.ImGui/AbstUI.ImGui.csproj | ImGui backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest/AbstUI.GfxVisualTest.csproj | Shared graphics visual test utilities for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.ImGui/AbstUI.GfxVisualTest.ImGui.csproj | ImGui visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.SDL2/AbstUI.GfxVisualTest.SDL2.csproj | SDL2 visual test application for AbstUI |
| WillMoveToOwnRepo/ProjectorRays/src/ProjectorRays.DotNet/ProjectorRays.DotNet.csproj | Core ProjectorRays .NET library |
| WillMoveToOwnRepo/ProjectorRays/src/ProjectorRays.Console/ProjectorRays.Console.csproj | Console showcase for ProjectorRays |
| WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj | Tests for ProjectorRays library |
## Notes for Agents
- The solution file is `LingoEngine.sln`.
- Keep cross-platform compatibility in mind when making changes.
