using BlingoEngine.Transitions;

namespace BlingoEngine.Director.Core.Scores.Transitions;

internal class DirScoreTransitionSprite : DirScoreSprite<BlingoTransitionSprite>
{
    
    public DirScoreTransitionSprite(BlingoTransitionSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}

