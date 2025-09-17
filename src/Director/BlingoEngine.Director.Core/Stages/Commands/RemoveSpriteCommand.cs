using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Stages.Commands;

/// <summary>
/// Command for deleting a sprite from the score.
/// </summary>
public sealed record RemoveSpriteCommand(BlingoMovie Movie, BlingoSprite Sprite) : IAbstCommand;


