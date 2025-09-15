using AbstUI.Commands;
using LingoEngine.Projects;

namespace LingoEngine.Director.Core.Projects.Commands;

public sealed record SaveDirProjectSettingsCommand(
    DirectorProjectSettings? DirSettings = null,
    LingoProjectSettings? ProjectSettings = null) : IAbstCommand;
