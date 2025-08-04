using LingoEngine.ColorPalettes;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.Core.Scores.ColorPalettes;

internal class DirScoreColorPaletteSprite : DirScoreSprite<LingoColorPaletteSprite>
{
    public const int FrameScriptSpriteNum = 2;
    public DirScoreColorPaletteSprite(LingoColorPaletteSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }

    internal LingoColorPaletteFrameSettings? GetSettings() => SpriteT.GetSettings();
}
