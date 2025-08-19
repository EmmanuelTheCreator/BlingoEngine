using AbstUI.Commands;

namespace LingoEngine.Director.Core.Stages.Commands;

public sealed record StageToolSelectCommand(StageTool Tool) : IAbstCommand;
