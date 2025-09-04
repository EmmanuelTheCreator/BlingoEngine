using UnityEngine;
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
            => _loadedFonts.TryGetValue((name, style), out var f) ? f as T : null;

        public Font GetTyped(string name, AbstFontStyle style = AbstFontStyle.Regular) => _loadedFonts[(name, style)];

    private Font _defaultFont = UnityEngine.Resources.GetBuiltinResource<Font>("Tahoma.ttf");

    public T GetDefaultFont<T>() where T : class => (_defaultFont as T)!;

    public void SetDefaultFont<T>(T font) where T : class => _defaultFont = (font as Font)!;

        public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public float MeasureTextWidth(string text, string fontName, int fontSize)
    {
            var font = string.IsNullOrEmpty(fontName) ? _defaultFont :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out var f) ? f : _defaultFont);
        font.RequestCharactersInTexture(text, fontSize, FontStyle.Normal);
        float width = 0f;
        foreach (var ch in text)
        {
            if (font.GetCharacterInfo(ch, out var info, fontSize))
                width += info.advance;
        }
        return width;
    }

    public FontInfo GetFontInfo(string fontName, int fontSize)
    {
            var font = string.IsNullOrEmpty(fontName) ? _defaultFont :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out var f) ? f : _defaultFont);
        font.RequestCharactersInTexture(" ", fontSize, FontStyle.Normal);
        font.GetCharacterInfo(' ', out var info, fontSize);
        int height = Mathf.CeilToInt(info.glyphHeight);
        int top = Mathf.CeilToInt(info.glyphHeight - info.maxY);
        return new FontInfo(height, top);
    }
}
