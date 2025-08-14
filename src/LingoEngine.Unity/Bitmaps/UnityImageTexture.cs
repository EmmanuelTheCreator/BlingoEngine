using LingoEngine.Bitmaps;
using UnityEngine;

namespace LingoEngine.Unity.Bitmaps;

public class UnityTexture2D : ILingoTexture2D
{
    public Texture2D Texture { get; }

    public UnityTexture2D(Texture2D texture)
    {
        Texture = texture;
    }

    public int Width => Texture.width;
    public int Height => Texture.height;
}
