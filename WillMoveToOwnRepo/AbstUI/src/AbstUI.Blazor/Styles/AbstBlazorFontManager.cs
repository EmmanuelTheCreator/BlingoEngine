using System;
using System.Collections.Generic;
using AbstUI.Styles;
using System.Linq;

namespace AbstUI.Blazor.Styles;

public class AbstBlazorFontManager : AbstFontManagerBase
{
    private readonly List<(string Name, AbstFontStyle Style, string File)> _fontsToLoad = new();
    private readonly Dictionary<(string Name, AbstFontStyle Style), string> _loadedFonts = new();
    private string _defaultFont = "sans-serif";

    public event Action? FontsChanged;

    public override IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
    {
        _fontsToLoad.Add((name, style, pathAndName));
        return this;
    }

    public override void LoadAll()
    {
        foreach (var font in _fontsToLoad)
            _loadedFonts[(font.Name, font.Style)] = font.File;
        _fontsToLoad.Clear();
        FontsChanged?.Invoke();
    }

    public override T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
    {
        if (string.IsNullOrEmpty(name))
            return _defaultFont as T;
        if (_loadedFonts.TryGetValue((name, style), out var font))
            return font as T;
        if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out font))
            return font as T;
        return _defaultFont as T;
    }

    public override T GetDefaultFont<T>() where T : class
        => (_defaultFont as T)!;

    public override void SetDefaultFont<T>(T font) where T : class
        => _defaultFont = font?.ToString() ?? string.Empty;

    public override IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public override float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        _ = Get<string>(fontName, style);
        return text.Length * fontSize * 0.6f;
    }

    public override FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        _ = Get<string>(fontName, style);
        return new(fontSize, fontSize);
    }

}
