using System;
using System.Collections.Generic;
using AbstUI.Styles;

namespace LingoEngine.Tests;

internal sealed class TestFontManager : IAbstFontManager
{
    public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular) => this;
    public void LoadAll() { }
    public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class => null;
    public T GetDefaultFont<T>() where T : class => null!;
    public void SetDefaultFont<T>(T font) where T : class { }
    public IEnumerable<string> GetAllNames() => Array.Empty<string>();
    public float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        => text.Length * fontSize;
    public FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        => new(fontSize, 0);
}
