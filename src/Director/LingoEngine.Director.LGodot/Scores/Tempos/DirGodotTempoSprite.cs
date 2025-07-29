using LingoEngine.Sounds;
using LingoEngine.Tempos;

namespace LingoEngine.Director.LGodot.Scores.Tempos;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirGodotTempoSprite : DirGodotTopSprite<LingoTempoSprite>
{
    public DirGodotTempoSprite(LingoTempoSprite sprite, Core.Sprites.IDirSpritesManager spritesManager)
    {
        SpritesManager = spritesManager;
        Init(sprite);
    }
}
