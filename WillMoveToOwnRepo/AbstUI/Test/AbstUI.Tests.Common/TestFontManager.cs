using AbstUI.Styles;

namespace AbstUI.Tests.Common;

public class TestFontManager : IAbstFontManager
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

    public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular) => this;
    public void LoadAll() { }
    public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class => null;
    public T GetDefaultFont<T>() where T : class => null!;
    public void SetDefaultFont<T>(T font) where T : class { }
    public IEnumerable<string> GetAllNames() => Array.Empty<string>();
    public float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular) => text.Length * fontSize;
    public FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        int ascent = _topIndents.TryGetValue(fontName, out var ti) ? ti : _topIndent;
        int extra = _extraHeights.TryGetValue(fontName, out var h) ? h : 0;
        return new(fontSize + extra, ascent);

    }
}
