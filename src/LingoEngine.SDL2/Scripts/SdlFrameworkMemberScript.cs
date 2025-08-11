using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Scripts;

public class SdlFrameworkMemberScript : ILingoFrameworkMemberScript
{
    public bool IsLoaded => true;
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload() { }
    public void Unload() { }
}
