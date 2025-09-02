using System;

namespace AbstUI.Primitives;

public interface IAbstUITextureUserSubscription
{
    IAbstTexture2D Texture { get; }
    void Release();
}

public interface IAbstTexture2D : IDisposable
{
    int Width { get; }

    int Height { get; }
    public bool IsDisposed { get; }
    string Name { get; set; }

    IAbstUITextureUserSubscription AddUser(object user);

    /// <summary>
    /// Returns the pixel data in ARGB byte order.
    /// </summary>
    byte[] GetPixels();

    /// <summary>
    /// Replaces the pixel data using the provided ARGB byte array.
    /// </summary>
    void SetARGBPixels(byte[] argbPixels);

    /// <summary>
    /// Replaces the pixel data using the provided RGBA byte array.
    /// </summary>
    void SetRGBAPixels(byte[] rgbaPixels);

    IAbstTexture2D Clone();

}

