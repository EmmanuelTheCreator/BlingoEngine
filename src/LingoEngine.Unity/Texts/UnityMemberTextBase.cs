using System.IO;
using AbstUI.Texts;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Unity.Texts;

/// <summary>
/// Shared implementation for text-based members in the Unity backend.
/// </summary>
public abstract class UnityMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    protected string _text = string.Empty;
    protected readonly IAbstFontManager _fontManager;

    protected UnityMemberTextBase(IAbstFontManager fontManager)
    {
        _fontManager = fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
    }

    public string Text { get => _text; set => _text = value; }
    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int FontSize { get; set; }
    public LingoTextStyle FontStyle { get; set; }
    public AColor TextColor { get; set; } = AColor.FromRGB(0, 0, 0);
    public AbstTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public IAbstTexture2D? TextureLingo => throw new NotImplementedException();

    public void Copy(string text) { }
    public string PasteClipboard() => string.Empty;

    public string ReadText() => File.Exists(_lingoMemberText.FileName) ? File.ReadAllText(_lingoMemberText.FileName) : string.Empty;
    public string ReadTextRtf()
    {
        var rtf = Path.ChangeExtension(_lingoMemberText.FileName, ".rtf");
        return File.Exists(rtf) ? File.ReadAllText(rtf) : string.Empty;
    }

    public void CopyToClipboard() { }
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { _lingoMemberText.Text = string.Empty; }
    public void Preload() { IsLoaded = true; }
    public void Unload() { IsLoaded = false; }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void Dispose() { }

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        throw new NotImplementedException();
    }
}
