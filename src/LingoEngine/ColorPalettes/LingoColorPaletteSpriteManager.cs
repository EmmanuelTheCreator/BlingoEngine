using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.ColorPalettes;
public interface ILingoSpriteColorPaletteSpriteManager : ILingoSpriteManager<LingoColorPaletteSprite>
{
    LingoColorPaletteSprite Add(int frameNumber, LingoColorPaletteFrameSettings settings);
}
internal class LingoSpriteColorPaletteSpriteManager : LingoSpriteManager<LingoColorPaletteSprite>, ILingoSpriteColorPaletteSpriteManager
{

    public LingoSpriteColorPaletteSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
    {
    }

    public LingoColorPaletteSprite Add(int frame) => AddSprite(1, "ColorPalette_" + frame, sprite => sprite.Frame = frame);



    protected override LingoColorPaletteSprite OnCreateSprite(LingoMovie movie, Action<LingoColorPaletteSprite> onRemove) => new LingoColorPaletteSprite(_environment, onRemove);


    public LingoColorPaletteSprite Add(int frameNumber, LingoColorPaletteFrameSettings settings)
    {
        var sprite = AddSprite(1, "ColorPalette_" + frameNumber, c => c.SetSettings(settings));
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        return sprite;
    }
}
