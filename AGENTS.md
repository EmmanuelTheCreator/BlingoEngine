# AGENTS

These instructions apply to the entire repository.

## Environment
- The project targets **.NET 8**. If the `dotnet` CLI isn't available, install it with `./scripts/install-dotnet.sh` and ensure `$HOME/.dotnet` is on your `PATH`.

## Testing
- Run tests only for the projects affected by your changes; do not run the entire solution.
- For changes in core engine code (e.g., under `src` or `Test/LingoEngine.Lingo.Core.Tests`), run `dotnet test Test/LingoEngine.Lingo.Core.Tests/LingoEngine.Lingo.Core.Tests.csproj`.
- For changes in the ProjectorRays area (`WillMoveToOwnRepo/ProjectorRays`), run `dotnet test WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj`.
- Apply the same approach for other components: run `dotnet test <path-to-test-project>` for each modified project.

## Code Style
- Use `dotnet format` to fix style issues when needed.
- Prefer `rg` (ripgrep) over `grep` for searching the codebase.

## Notes for Agents
- The solution file is `LingoEngine.sln`.
- Keep cross-platform compatibility in mind when making changes.
