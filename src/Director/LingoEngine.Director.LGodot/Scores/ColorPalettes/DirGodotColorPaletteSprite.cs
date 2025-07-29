using LingoEngine.ColorPalettes;

namespace LingoEngine.Director.LGodot.Scores.ColorPalettes;

internal class DirGodotColorPaletteSprite : DirGodotTopSprite<LingoColorPaletteSprite>
{
    public DirGodotColorPaletteSprite(LingoColorPaletteSprite sprite, Core.Sprites.IDirSpritesManager spritesManager)
    {
        SpritesManager = spritesManager;
        Init(sprite);
    }
}
