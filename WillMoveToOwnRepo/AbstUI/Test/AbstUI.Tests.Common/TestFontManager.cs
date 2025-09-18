using AbstUI.Styles;

namespace AbstUI.Tests.Common;

public class TestFontManager : AbstFontManagerBase
{
    private readonly int _topIndent;
    private readonly Dictionary<string, int> _topIndents;
    private readonly Dictionary<string, int> _extraHeights;

    public TestFontManager(int topIndent = 0, Dictionary<string, int>? topIndents = null, Dictionary<string, int>? extraHeights = null)
    {
        _topIndent = topIndent;
        _topIndents = topIndents ?? new();
        _extraHeights = extraHeights ?? new();
    }

    public override IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular) => this;
    public override void LoadAll() { }
    public override T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class => null;
    public override T GetDefaultFont<T>() where T : class => null!;
    public override void SetDefaultFont<T>(T font) where T : class { }
    public override IEnumerable<string> GetAllNames() => Array.Empty<string>();
    public override float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular) => text.Length * fontSize;
    public override FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        int ascent = _topIndents.TryGetValue(fontName, out var ti) ? ti : _topIndent;
        int extra = _extraHeights.TryGetValue(fontName, out var h) ? h : 0;
        return new(fontSize + extra, ascent);

    }
}
