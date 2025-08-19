using AbstUI.Commands;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Inspector.Commands;

public sealed record OpenBehaviorPopupCommand(LingoSpriteBehavior Behavior) : IAbstCommand;
