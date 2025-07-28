using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.ColorPalettes;

public class LingoColorPaletteSprite : LingoSprite
{
    private readonly Action<LingoColorPaletteSprite> _removeMe;

    public int Frame { get; set; }
    public int ColorPaletteId { get; set; }
    public LingoColorPaletteSprite(ILingoMovieEnvironment environment, Action<LingoColorPaletteSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
    }

    public override void RemoveMe()
    {
        _removeMe(this);
    }
}
