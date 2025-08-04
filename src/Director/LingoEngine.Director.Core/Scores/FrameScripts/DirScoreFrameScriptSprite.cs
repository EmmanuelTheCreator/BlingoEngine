using LingoEngine.Director.Core.Scores;
using LingoEngine.Scripts;

namespace LingoEngine.Director.Core.Scores.FrameScripts;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreFrameScriptSprite : DirScoreSprite<LingoSpriteFrameScript>
{
    public DirScoreFrameScriptSprite(LingoSpriteFrameScript sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}
