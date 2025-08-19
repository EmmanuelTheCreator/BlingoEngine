using AbstUI.Primitives;
using AbstUI.Commands;

namespace LingoEngine.Director.Core.Stages.Commands;

public sealed record StageChangeBackgroundColorCommand(AColor OldColor, AColor NewColor) : IAbstCommand;
