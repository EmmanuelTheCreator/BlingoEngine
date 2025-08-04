using LingoEngine.Sounds;

namespace LingoEngine.Director.Core.Scores.Sounds;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreSoundSprite : DirScoreSprite<LingoSpriteSound>
{
    public const int FrameScriptSpriteNumOffset = 3;
    public DirScoreSoundSprite(LingoSpriteSound sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}
