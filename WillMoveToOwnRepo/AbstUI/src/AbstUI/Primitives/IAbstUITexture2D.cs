namespace AbstUI.Primitives;

public interface IAbstUITextureUserSubscription
{
    IAbstUITexture2D Texture { get; }
    void Release();
}

public interface IAbstUITexture2D : IDisposable
{
    int Width { get; }

    int Height { get; }
    public bool IsDisposed { get; }
    string Name { get; set; }

    IAbstUITextureUserSubscription AddUser(object user);

}

