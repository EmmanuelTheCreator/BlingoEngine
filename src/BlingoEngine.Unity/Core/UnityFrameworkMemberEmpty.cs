using BlingoEngine.Members;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.Unity.Core;

public class UnityFrameworkMemberEmpty : IBlingoFrameworkMemberEmpty
{
    public bool IsLoaded => true;

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

    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }

    public void Unload() { }
    public bool IsPixelTransparent(int x, int y) => false;
}

