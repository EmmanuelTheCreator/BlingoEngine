using System.Reflection.Metadata;

namespace LingoEngine.Bitmaps;

public interface ILingoTextureUserSubscription
{
    ILingoTexture2D Texture { get; }
    void Release();
}

public interface ILingoTexture2D : IDisposable
{
    int Width { get; }

    int Height { get; }
    public bool IsDisposed{        get;}
    string Name { get; set; }

    ILingoTextureUserSubscription AddUser(object user);

}

