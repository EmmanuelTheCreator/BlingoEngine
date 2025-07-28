using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.ColorPalettes;
public interface ILingoSpriteColorPaletteManager : ILingoSpriteManager<LingoColorPaletteSprite>
{
    LingoColorPaletteSprite Add(int frame);
}
internal class LingoSpriteColorPaletteManager : LingoSpriteManager<LingoColorPaletteSprite>, ILingoSpriteColorPaletteManager
{

    public LingoSpriteColorPaletteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
    {
    }

    public LingoColorPaletteSprite Add(int frame) => AddSprite(1, "ColorPalette_" + frame, sprite => sprite.Frame = frame);

    protected override LingoColorPaletteSprite OnCreateSprite(LingoMovie movie, Action<LingoColorPaletteSprite> onRemove) => new LingoColorPaletteSprite(_environment, onRemove);
}
