using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Stages.Commands;

public sealed record StageToolSelectCommand(StageTool Tool) : IAbstCommand;

