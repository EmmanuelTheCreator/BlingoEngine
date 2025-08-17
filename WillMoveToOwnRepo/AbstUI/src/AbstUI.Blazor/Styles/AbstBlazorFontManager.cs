using System;
using AbstUI.Styles;

namespace AbstUI.Blazor.Styles;

public class AbstBlazorFontManager : IAbstFontManager
{
    private readonly List<(string Name, string File)> _fontsToLoad = new();
    private readonly Dictionary<string, string> _loadedFonts = new();
    private string _defaultFont = "sans-serif";

    public event Action? FontsChanged;

    public IAbstFontManager AddFont(string name, string pathAndName)
    {
        _fontsToLoad.Add((name, pathAndName));
        return this;
    }

    public void LoadAll()
    {
        foreach (var font in _fontsToLoad)
            _loadedFonts[font.Name] = font.File;
        _fontsToLoad.Clear();
        FontsChanged?.Invoke();
    }

    public T? Get<T>(string name) where T : class
        => _loadedFonts.TryGetValue(name, out var font) ? font as T : null;

    public T GetDefaultFont<T>() where T : class
        => (_defaultFont as T)!;

    public void SetDefaultFont<T>(T font) where T : class
        => _defaultFont = font?.ToString() ?? string.Empty;

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys;
}
