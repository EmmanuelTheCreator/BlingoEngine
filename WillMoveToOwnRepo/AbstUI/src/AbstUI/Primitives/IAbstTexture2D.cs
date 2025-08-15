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

}

