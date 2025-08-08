using LingoEngine.Commands;
using LingoEngine.Projects;

namespace LingoEngine.Director.Core.Projects.Commands;

public sealed record SaveDirProjectSettingsCommand(
    DirectorProjectSettings DirSettings,
    LingoProjectSettings ProjectSettings) : ILingoCommand;
