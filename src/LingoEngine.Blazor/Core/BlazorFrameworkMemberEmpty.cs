using LingoEngine.Members;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.Blazor.Core;

/// <summary>
/// Minimal backend implementation for member types that require no platform features.
/// </summary>
public class BlazorFrameworkMemberEmpty : ILingoFrameworkMemberEmpty
{
    public bool IsLoaded => true;

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload()
    {
        if (IsLoaded)
            return;
    }
    public Task PreloadAsync() => Task.CompletedTask;
    public void Unload() { }

    public bool IsPixelTransparent(int x, int y) => false;
}
