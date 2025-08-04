using LingoEngine.Director.Core.Scores;
using LingoEngine.Transitions;

namespace LingoEngine.Director.Core.Scores.Transitions;

internal class DirScoreTransitionSprite : DirScoreSprite<LingoTransitionSprite>
{
    public const int FrameScriptSpriteNum = 3;
    public DirScoreTransitionSprite(LingoTransitionSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}
