using BlingoEngine.Tempos;

namespace BlingoEngine.Director.Core.Scores.Tempos;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreTempoSprite : DirScoreSprite<BlingoTempoSprite>
{
    
    public DirScoreTempoSprite(BlingoTempoSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}

