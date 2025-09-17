using AbstUI.Commands;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Inspector.Commands;

public sealed record OpenBehaviorPopupCommand(BlingoSpriteBehavior Behavior) : IAbstCommand;

