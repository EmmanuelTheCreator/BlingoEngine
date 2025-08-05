using LingoEngine.Movies;
using LingoEngine.ColorPalettes;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.Core.Scores.ColorPalettes;


internal partial class DirScoreColorPaletteGridChannel : DirScoreChannel<ILingoSpriteColorPaletteSpriteManager, DirScoreColorPaletteSprite, LingoColorPaletteSprite>
{
    private int _editFrame;
    private LingoColorPaletteFrameSettings? _currentSettings;
    public DirScoreColorPaletteGridChannel(IDirScoreManager scoreManager)
        : base(LingoColorPaletteSprite.SpriteNumOffset+1, scoreManager)
    {

    }

    protected override DirScoreColorPaletteSprite CreateUISprite(LingoColorPaletteSprite sprite, IDirSpritesManager spritesManager) => new DirScoreColorPaletteSprite(sprite, spritesManager);

    protected override ILingoSpriteColorPaletteSpriteManager GetManager(LingoMovie movie) => movie.ColorPalettes;


   
}
