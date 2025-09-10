using UnityEngine;
using System.Collections.Generic;
using AbstUI.Styles;
using System.Linq;

namespace AbstUI.LUnity.Styles;

/// <summary>
/// Basic font manager that loads Unity <see cref="Font"/> assets and exposes them to the engine.
/// </summary>
internal class UnityFontManager : IAbstFontManager
{
    private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<(string Name, AbstFontStyle Style), Font> _loadedFonts = new();

    public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
    {
        _fontsToLoad.Add((name, style, pathAndName));
        return this;
    }

    public void LoadAll()
    {
        foreach (var font in _fontsToLoad)
        {
            // Try loading from Resources first; fall back to built-in Tahoma

            var loaded = UnityEngine.Resources.Load<Font>(font.FileName) ?? UnityEngine.Resources.GetBuiltinResource<Font>("Tahoma.ttf");
            _loadedFonts[(font.Name, font.Style)] = loaded;
        }
        _fontsToLoad.Clear();
    }

    public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
    {
        if (string.IsNullOrEmpty(name))
            return _defaultFont as T;
        if (_loadedFonts.TryGetValue((name, style), out var f))
            return f as T;
        if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out f))
            return f as T;
        return _defaultFont as T;
    }

    public Font GetTyped(string name, AbstFontStyle style = AbstFontStyle.Regular)
    {
        if (string.IsNullOrEmpty(name))
            return _defaultFont;
        if (_loadedFonts.TryGetValue((name, style), out var f))
            return f;
        if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out f))
            return f;
        return _defaultFont;
    }

    private Font _defaultFont = UnityEngine.Resources.GetBuiltinResource<Font>("Tahoma.ttf");

    public T GetDefaultFont<T>() where T : class => (_defaultFont as T)!;

    public void SetDefaultFont<T>(T font) where T : class => _defaultFont = (font as Font)!;

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var font = string.IsNullOrEmpty(fontName) ? _defaultFont :
            (_loadedFonts.TryGetValue((fontName, style), out var f) ? f :
            (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out f) ? f : _defaultFont));
        var unityStyle = FontStyle.Normal;
        if ((style & AbstFontStyle.Bold) != 0) unityStyle |= FontStyle.Bold;
        if ((style & AbstFontStyle.Italic) != 0) unityStyle |= FontStyle.Italic;
        font.RequestCharactersInTexture(text, fontSize, unityStyle);
        float width = 0f;
        foreach (var ch in text)
        {
            if (font.GetCharacterInfo(ch, out var info, fontSize, unityStyle))
                width += info.advance;
        }
        return width;
    }

    public FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var font = string.IsNullOrEmpty(fontName) ? _defaultFont :
            (_loadedFonts.TryGetValue((fontName, style), out var f) ? f :
            (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out f) ? f : _defaultFont));
        var unityStyle = FontStyle.Normal;
        if ((style & AbstFontStyle.Bold) != 0) unityStyle |= FontStyle.Bold;
        if ((style & AbstFontStyle.Italic) != 0) unityStyle |= FontStyle.Italic;
        font.RequestCharactersInTexture(" ", fontSize, unityStyle);
        font.GetCharacterInfo(' ', out var info, fontSize, unityStyle);
        int height = Mathf.CeilToInt(info.glyphHeight);
        int top = Mathf.CeilToInt(info.maxY);
        return new FontInfo(height, top);
    }
}
