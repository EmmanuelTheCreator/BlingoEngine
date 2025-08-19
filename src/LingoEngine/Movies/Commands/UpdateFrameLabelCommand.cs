using AbstUI.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record UpdateFrameLabelCommand(int PreviousFrame, int NewFrame, string Name) : IAbstCommand;
public sealed record DeleteFrameLabelCommand(int Frame) : IAbstCommand;
