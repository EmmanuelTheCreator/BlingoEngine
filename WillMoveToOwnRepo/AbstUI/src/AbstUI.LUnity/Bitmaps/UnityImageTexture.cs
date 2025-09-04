using AbstUI.Bitmaps;
using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Bitmaps;

public class UnityTexture2D : AbstBaseTexture2D<Texture2D>
{
    public Texture2D? Texture { get; private set; }

    public UnityTexture2D(Texture2D texture, string name = "") : base(name)
    {
        Texture = texture;
    }

    public override int Width => Texture?.width ?? 0;
    public override int Height => Texture?.height ?? 0;

    public Sprite? ToSprite()
    {
        if (Texture == null)
            return null;
        return Sprite.Create(Texture, new Rect(0, 0, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));
    }

    protected override void DisposeTexture()
    {
        if (Texture != null)
        {
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }

    public override byte[] GetPixels()
    {
        throw new NotImplementedException();
    }

    public override void SetARGBPixels(byte[] argbPixels)
    {
        throw new NotImplementedException();
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        throw new NotImplementedException();
    }

    public override IAbstTexture2D Clone()
    {
        throw new NotImplementedException();
    }
}
