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
        if (img == null) return Array.Empty<byte>();
        img.Convert(Image.Format.Rgba8);              // ensure RGBA8
        return img.GetData();                         // return RGBA (no channel swap)
    }


    public override IAbstTexture2D Clone()
    {
        // Get the pixel data from the existing texture
        Image img = _texture.GetImage(); // This returns a copy of the image data
        ImageTexture newTex = ImageTexture.CreateFromImage(img);

        return new AbstGodotTexture2D(newTex,Name+"_Clone");
    }

    public override void SetARGBPixels(byte[] argbPixels)
    {
        if (argbPixels == null || argbPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected ARGB8888 buffer.", nameof(argbPixels));

        APixel.ToRGBA(argbPixels);                    // ARGB -> RGBA (in place)
        using var img = Image.CreateFromData(Width, Height, false, Image.Format.Rgba8, argbPixels);
        ((ImageTexture)_texture).Update(img);
    }


    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        if (rgbaPixels == null || rgbaPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected RGBA8888 buffer.", nameof(rgbaPixels));

        using var img = Image.CreateFromData(Width, Height, false, Image.Format.Rgba8, rgbaPixels);
        ((ImageTexture)_texture).Update(img);
    }


    public static AbstGodotTexture2D FromARGBPixels(int width, int height, byte[] argbPixels, string? name = null)
    {
        APixel.ToRGBA(argbPixels);
        var img = Image.CreateFromData(width, height, false, Image.Format.Rgba8, argbPixels);
        var tex = ImageTexture.CreateFromImage(img);
        return new AbstGodotTexture2D(tex, name ?? string.Empty);
    }

#if DEBUG
    private static int _incrementerDebug = 0;

    public void DebugWriteToDiskInc()
    {
        _incrementerDebug++;
        DebugToDisk(Texture, $"{Name}_{_incrementerDebug}");
    }

    public void DebugWriteToDisk()
        => DebugToDisk(Texture, Name);

    public static void DebugToDisk(Texture2D texture, string fileName)
        => DebugToDisk(texture, "", fileName);

    public static void DebugToDisk(Texture2D texture, string folder, string fileName)
    {
        if (texture == null)
            throw new Exception("DebugToDisk: texture is null.");

        var fn = $"C:/temp/director/{(!string.IsNullOrWhiteSpace(folder) ? folder + "/" : "")}Godot_{fileName}.png";
        if (File.Exists(fn)) File.Delete(fn);

        var img = texture.GetImage();
        img.Convert(Image.Format.Rgba8); // ensure standard format
        img.SavePng(fn);
    }
#endif

}
