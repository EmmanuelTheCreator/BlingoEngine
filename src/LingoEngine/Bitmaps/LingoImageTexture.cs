namespace LingoEngine.Bitmaps;

public interface ILingoTextureUserSubscription
{
    void Release();
}

public interface ILingoTexture2D
{
    int Width { get; }

    int Height { get; }

    ILingoTextureUserSubscription AddUser(object user);
}

