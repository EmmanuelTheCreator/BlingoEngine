using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Scripts;

namespace BlingoEngine.Director.Core.Scores.FrameScripts;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreFrameScriptSprite : DirScoreSprite<BlingoFrameScriptSprite>
{
    public DirScoreFrameScriptSprite(BlingoFrameScriptSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}

