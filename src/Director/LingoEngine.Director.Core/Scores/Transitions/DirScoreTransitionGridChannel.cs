using LingoEngine.Movies;
using LingoEngine.Transitions;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core.Scores.Transitions;

internal partial class DirScoreTransitionGridChannel : DirScoreChannel<ILingoSpriteTransitionManager, DirScoreTransitionSprite, LingoTransitionSprite>
{
    public DirScoreTransitionGridChannel(IDirScoreManager scoreManager)
        : base(LingoTransitionSprite.SpriteNumOffset+1, scoreManager)
    {
        IsSingleFrame = true;
    }

    protected override DirScoreTransitionSprite CreateUISprite(LingoTransitionSprite sprite, IDirSpritesManager spritesManager) => new DirScoreTransitionSprite(sprite, spritesManager);

    protected override ILingoSpriteTransitionManager GetManager(LingoMovie movie) => movie.Transitions;
}
