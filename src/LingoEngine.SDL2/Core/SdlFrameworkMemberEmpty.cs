using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Core;

public class SdlFrameworkMemberEmpty : ILingoFrameworkMemberEmpty
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
