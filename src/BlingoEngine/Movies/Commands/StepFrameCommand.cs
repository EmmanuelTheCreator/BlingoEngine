using AbstUI.Commands;

namespace BlingoEngine.Movies.Commands;

public sealed record StepFrameCommand(int Offset) : IAbstCommand;

