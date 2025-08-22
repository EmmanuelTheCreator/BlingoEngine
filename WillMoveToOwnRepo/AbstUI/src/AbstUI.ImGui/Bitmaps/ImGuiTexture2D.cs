using System;
using AbstUI.Bitmaps;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Bitmaps;

/// <summary>
/// Minimal texture wrapper with no native dependencies.
/// </summary>
public class ImGuiTexture2D : AbstBaseTexture2D<nint>
{
    public nint Handle { get; private set; }
    public override int Width { get; }
    public override int Height { get; }

    public ImGuiTexture2D(nint texture, int width, int height, string name = "") : base(name)
    {
        Handle = texture;
        Width = width;
        Height = height;
    }

    protected override void DisposeTexture()
    {
        Handle = nint.Zero;
        // TODO: release texture resources
    }

    public nint ToSurface(nint renderer, out int w, out int h, uint? fmt = null)
    {
        w = Width;
        h = Height;
        // TODO: convert texture to a CPU surface without platform APIs
        return nint.Zero;
    }

    public IAbstTexture2D Clone(nint renderer)
        => throw new NotImplementedException();

#if DEBUG
    public void DebugWriteToDisk(nint renderer) => throw new NotImplementedException();
    public static void DebugToDisk(nint renderer, nint texture, string fileName) => throw new NotImplementedException();
    public static void DebugToDisk(nint renderer, nint texture, string folder, string fileName) => throw new NotImplementedException();
#endif
}
