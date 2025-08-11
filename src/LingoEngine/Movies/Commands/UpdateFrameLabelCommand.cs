using LingoEngine.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record UpdateFrameLabelCommand(int PreviousFrame, int NewFrame, string Name) : ILingoCommand;
public sealed record DeleteFrameLabelCommand(int Frame) : ILingoCommand;
