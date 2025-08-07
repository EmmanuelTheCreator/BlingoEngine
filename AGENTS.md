# AGENTS

These instructions apply to the entire repository.

## Environment
- The project targets **.NET 8**. If the `dotnet` CLI isn't available, install it with `./scripts/install-dotnet.sh` and ensure `$HOME/.dotnet` is on your `PATH`.

## Testing
- Run `dotnet test` from the repository root before committing any change.

## Code Style
- Use `dotnet format` to fix style issues when needed.
- Prefer `rg` (ripgrep) over `grep` for searching the codebase.

## Notes for Agents
- The solution file is `LingoEngine.sln`.
- Keep cross-platform compatibility in mind when making changes.
