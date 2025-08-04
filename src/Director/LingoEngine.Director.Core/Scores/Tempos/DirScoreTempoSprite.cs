using LingoEngine.Tempos;

namespace LingoEngine.Director.Core.Scores.Tempos;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreTempoSprite : DirScoreSprite<LingoTempoSprite>
{
    public const int FrameScriptSpriteNum = 1;
    public DirScoreTempoSprite(LingoTempoSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}
