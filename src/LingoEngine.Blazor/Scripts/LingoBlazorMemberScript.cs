using LingoEngine.Scripts;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.Blazor.Scripts;

/// <summary>
/// Minimal script member implementation for the Blazor backend.
/// </summary>
public class LingoBlazorMemberScript : ILingoFrameworkMemberScript
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
    public Task PreloadAsync()
    {
        return Task.CompletedTask;
    }
    public void Unload() { }
    public bool IsPixelTransparent(int x, int y) => false;
}
