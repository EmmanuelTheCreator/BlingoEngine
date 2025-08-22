using AbstUI.Bitmaps;
using AbstUI.Primitives;
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

    public override int Height => _texture._GetHeight();

    protected override void DisposeTexture()
    {
        _texture.Dispose();
    }

    public IAbstTexture2D Clone()
    {
        // Get the pixel data from the existing texture
        Image img = _texture.GetImage(); // This returns a copy of the image data
        ImageTexture newTex = ImageTexture.CreateFromImage(img);

        return new AbstGodotTexture2D(newTex);
    }
}
