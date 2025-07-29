using LingoEngine.Scripts;

namespace LingoEngine.Director.LGodot.Scores.FrameScripts;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirGodotFrameScriptSprite : DirGodotTopSprite<LingoSpriteFrameScript>
{
    public DirGodotFrameScriptSprite(LingoSpriteFrameScript sprite, Core.Sprites.IDirSpritesManager spritesManager)
    {
        SpritesManager = spritesManager;
        Init(sprite);
    }
}
