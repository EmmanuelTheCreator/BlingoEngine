using AbstUI.Primitives;
using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Stages.Commands;

public sealed record StageChangeBackgroundColorCommand(AColor OldColor, AColor NewColor) : IAbstCommand;

