using AbstUI.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Tools;
using Godot;

namespace AbstUI.LGodot.Bitmaps;

public class AbstGodotTexture2D : AbstBaseTexture2D<Texture2D>
{
    private readonly Texture2D _texture;
    public Texture2D Texture => _texture;

    public AbstGodotTexture2D(Texture2D imageTexture, string name = "") : base(name)
    {
        _texture = imageTexture;
    }

    public override int Width => _texture.GetWidth();

    public override int Height => _texture.GetHeight();

    protected override void DisposeTexture()
    {
        _texture.Dispose();
    }

    public override byte[] GetPixels()
    {
        var img = _texture.GetImage();
        img.Convert(Image.Format.Rgba8);
        var rgba = img.GetData();
        var argb = new byte[rgba.Length];
        for (int i = 0; i < rgba.Length; i += 4)
        {
            argb[i] = rgba[i + 3];
            argb[i + 1] = rgba[i];
            argb[i + 2] = rgba[i + 1];
            argb[i + 3] = rgba[i + 2];
        }
        return argb;
    }

    public IAbstTexture2D Clone()
    {
        // Get the pixel data from the existing texture
        Image img = _texture.GetImage(); // This returns a copy of the image data
        ImageTexture newTex = ImageTexture.CreateFromImage(img);

        return new AbstGodotTexture2D(newTex);
    }

    public override void SetARGBPixels(byte[] argbPixels)
    {
        APixel.ToRGBA(argbPixels);
        var img = Image.CreateFromData(Width, Height, false, Image.Format.Rgba8, argbPixels);
        ((ImageTexture)_texture).Update(img);
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        var img = Image.CreateFromData(Width, Height, false, Image.Format.Rgba8, rgbaPixels);
        ((ImageTexture)_texture).Update(img);
    }

    public static AbstGodotTexture2D FromARGBPixels(int width, int height, byte[] argbPixels, string? name = null)
    {
        APixel.ToRGBA(argbPixels);
        var img = Image.CreateFromData(width, height, false, Image.Format.Rgba8, argbPixels);
        var tex = ImageTexture.CreateFromImage(img);
        return new AbstGodotTexture2D(tex, name ?? string.Empty);
    }
}
