using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Stages.Commands;

public sealed record StageChangeBackgroundColorCommand(LingoColor OldColor, LingoColor NewColor) : ILingoCommand;
