using LingoEngine.Sounds;
using LingoEngine.Sprites;

namespace LingoEngine.Unity.Sounds;

/// <summary>
/// Unity stub implementation for sound cast members.
/// </summary>
public class UnityMemberSound : ILingoFrameworkMemberSound
{
    private LingoMemberSound _member = null!;

    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    internal void Init(LingoMemberSound member)
    {
        _member = member;
    }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void CopyToClipboard() { }
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload() { IsLoaded = true; }
    public void Unload() { IsLoaded = false; }
}
