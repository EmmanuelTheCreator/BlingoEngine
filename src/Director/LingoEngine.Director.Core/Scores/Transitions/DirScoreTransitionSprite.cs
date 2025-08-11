using LingoEngine.Transitions;

namespace LingoEngine.Director.Core.Scores.Transitions;

internal class DirScoreTransitionSprite : DirScoreSprite<LingoTransitionSprite>
{
    
    public DirScoreTransitionSprite(LingoTransitionSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}
