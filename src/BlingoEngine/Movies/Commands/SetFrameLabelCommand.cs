using AbstUI.Commands;

namespace BlingoEngine.Movies.Commands;

public sealed record SetFrameLabelCommand(int FrameNumber, string Name) : IAbstCommand;

