using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using System;

namespace BlingoEngine.ColorPalettes;
/// <summary>
/// Lingo Sprite Color Palette Sprite Manager interface.
/// </summary>
public interface IBlingoSpriteColorPaletteSpriteManager : IBlingoSpriteManager<BlingoColorPaletteSprite>
{
    BlingoColorPaletteSprite Add(int frameNumber, BlingoColorPaletteFrameSettings? settings = null);
}
internal class BlingoSpriteColorPaletteSpriteManager : BlingoSpriteManager<BlingoColorPaletteSprite>, IBlingoSpriteColorPaletteSpriteManager
{

    public BlingoSpriteColorPaletteSpriteManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoColorPaletteSprite.SpriteNumOffset, movie, environment)
    {
    }

    public BlingoColorPaletteSprite Add(int frame) => AddSprite(1, "ColorPalette_" + frame, sprite => sprite.Frame = frame);

    protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
    {
        var sprite = Add(begin);
        if (member is BlingoColorPaletteMember memberTyped)
            sprite.SetMember(memberTyped);
        return sprite;
    }

    protected override BlingoColorPaletteSprite OnCreateSprite(BlingoMovie movie, Action<BlingoColorPaletteSprite> onRemove) => new BlingoColorPaletteSprite(_environment.Events, _environment.CastLibsContainer.ActiveCast, onRemove);


    public BlingoColorPaletteSprite Add(int frameNumber, BlingoColorPaletteFrameSettings? settings = null)
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

