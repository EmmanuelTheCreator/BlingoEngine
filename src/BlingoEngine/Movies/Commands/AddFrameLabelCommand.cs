using AbstUI.Commands;

namespace BlingoEngine.Movies.Commands;

public sealed record AddFrameLabelCommand(int FrameNumber, string Name) : IAbstCommand;

