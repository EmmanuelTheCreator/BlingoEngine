namespace AbstUI.ImGui.Bitmaps;

public class ImGuiImageTexture : ImGuiTexture2D
{
    private nint _surfacePtr;
    public nint Ptr => _surfacePtr;
    public nint SurfaceId { get; private set; }

    public ImGuiImageTexture(nint surfacePtr, nint surfaceId, int width, int height)
        : base(nint.Zero, width, height)
    {
        _surfacePtr = surfacePtr;
        SurfaceId = surfaceId;
    }

}

