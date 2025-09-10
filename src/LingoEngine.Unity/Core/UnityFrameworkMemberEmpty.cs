using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.Unity.Core;

public class UnityFrameworkMemberEmpty : ILingoFrameworkMemberEmpty
{
    public bool IsLoaded => true;

    public void CopyToClipboard() { }

    public void Erase() { }

    public void ImportFileInto() { }

    public void PasteClipboardInto() { }

    public void Preload() { }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void Unload() { }
    public bool IsPixelTransparent(int x, int y) => false;
}
