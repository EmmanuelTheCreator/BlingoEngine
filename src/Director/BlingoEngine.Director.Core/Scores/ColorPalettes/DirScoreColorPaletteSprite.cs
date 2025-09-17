using BlingoEngine.ColorPalettes;
using BlingoEngine.Director.Core.Scores;

namespace BlingoEngine.Director.Core.Scores.ColorPalettes;

internal class DirScoreColorPaletteSprite : DirScoreSprite<BlingoColorPaletteSprite>
{
    
    public DirScoreColorPaletteSprite(BlingoColorPaletteSprite sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }

    internal BlingoColorPaletteFrameSettings? GetSettings() => SpriteT.GetSettings();
}

