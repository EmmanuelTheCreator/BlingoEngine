
using LingoEngine.Sounds;

namespace LingoEngine.Director.LGodot.Scores.Sounds;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirGodotSoundSprite : DirGodotTopSprite<LingoSpriteSound>
{
    public DirGodotSoundSprite(LingoSpriteSound sprite, Core.Sprites.IDirSpritesManager spritesManager)
    {
        SpritesManager = spritesManager;
        Init(sprite);
    }
}
