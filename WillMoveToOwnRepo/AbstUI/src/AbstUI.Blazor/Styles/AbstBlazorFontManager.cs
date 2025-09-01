using System;
using AbstUI.Styles;
using System.Linq;

namespace AbstUI.Blazor.Styles;

    public class AbstBlazorFontManager : IAbstFontManager
    {
        private readonly List<(string Name, AbstFontStyle Style, string File)> _fontsToLoad = new();
        private readonly Dictionary<(string Name, AbstFontStyle Style), string> _loadedFonts = new();
        private string _defaultFont = "sans-serif";

    public event Action? FontsChanged;

      public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
      {
          _fontsToLoad.Add((name, style, pathAndName));
          return this;
      }

    public void LoadAll()
    {
          foreach (var font in _fontsToLoad)
              _loadedFonts[(font.Name, font.Style)] = font.File;
        _fontsToLoad.Clear();
        FontsChanged?.Invoke();
    }

      public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
          => _loadedFonts.TryGetValue((name, style), out var font) ? font as T : null;

    public T GetDefaultFont<T>() where T : class
        => (_defaultFont as T)!;

    public void SetDefaultFont<T>(T font) where T : class
        => _defaultFont = font?.ToString() ?? string.Empty;

      public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public float MeasureTextWidth(string text, string fontName, int fontSize)
        => text.Length * fontSize * 0.6f;

    public FontInfo GetFontInfo(string fontName, int fontSize)
        => new(fontSize, 0);
}
