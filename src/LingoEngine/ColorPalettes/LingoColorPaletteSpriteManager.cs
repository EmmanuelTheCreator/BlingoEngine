using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using System;

namespace LingoEngine.ColorPalettes;
public interface ILingoSpriteColorPaletteSpriteManager : ILingoSpriteManager<LingoColorPaletteSprite>
{
    LingoColorPaletteSprite Add(int frameNumber, LingoColorPaletteFrameSettings? settings = null);
}
internal class LingoSpriteColorPaletteSpriteManager : LingoSpriteManager<LingoColorPaletteSprite>, ILingoSpriteColorPaletteSpriteManager
{

    public LingoSpriteColorPaletteSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoColorPaletteSprite.SpriteNumOffset, movie, environment)
    {
    }

    public LingoColorPaletteSprite Add(int frame) => AddSprite(1, "ColorPalette_" + frame, sprite => sprite.Frame = frame);

    protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member)
    {
        var sprite = Add(begin); 
        if (member is LingoColorPaletteMember memberTyped)
            sprite.SetMember(memberTyped);
        return sprite;
    }

    protected override LingoColorPaletteSprite OnCreateSprite(LingoMovie movie, Action<LingoColorPaletteSprite> onRemove) => new LingoColorPaletteSprite(_environment.Events, _environment.CastLibsContainer.ActiveCast, onRemove);


    public LingoColorPaletteSprite Add(int frameNumber, LingoColorPaletteFrameSettings? settings = null)
    {
        var sprite = AddSprite(1, "ColorPalette_" + frameNumber, c =>
        {
            if (settings != null)
                c.SetSettings(settings);
        });
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        return sprite;
    }

   
}
