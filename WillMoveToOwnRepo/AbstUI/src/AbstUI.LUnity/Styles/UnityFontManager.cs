using UnityEngine;
using AbstUI.Styles;

namespace AbstUI.LUnity.Styles;

/// <summary>
/// Basic font manager that loads Unity <see cref="Font"/> assets and exposes them to the engine.
/// </summary>
internal class UnityFontManager : IAbstFontManager
{
    private readonly List<(string Name, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<string, Font> _loadedFonts = new();

    public IAbstFontManager AddFont(string name, string pathAndName)
    {
        _fontsToLoad.Add((name, pathAndName));
        return this;
    }

    public void LoadAll()
    {
        foreach (var font in _fontsToLoad)
        {
            // Try loading from Resources first; fall back to built-in Arial
            var loaded = Resources.Load<Font>(font.FileName) ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            _loadedFonts[font.Name] = loaded;
        }
        _fontsToLoad.Clear();
    }

    public T? Get<T>(string name) where T : class
        => _loadedFonts.TryGetValue(name, out var f) ? f as T : null;

    public Font GetTyped(string name) => _loadedFonts[name];

    private Font _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

    public T GetDefaultFont<T>() where T : class => (_defaultFont as T)!;

    public void SetDefaultFont<T>(T font) where T : class => _defaultFont = (font as Font)!;

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys;
}
