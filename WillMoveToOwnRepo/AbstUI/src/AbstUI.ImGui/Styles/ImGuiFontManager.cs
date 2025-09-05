using System;
using System.Collections.Generic;
using System.IO;
using AbstUI.Styles;
using ImGuiNET;
using System.Linq;

namespace AbstUI.ImGui.Styles;

public interface IImGuiFontLoadedByUser
{
    nint FontHandle { get; }
    void Release();
}
public interface IAbstImGuiFont
{
    IImGuiFontLoadedByUser Get(object fontUser, int fontSize);
}

/// <summary>
/// Minimal font manager placeholder for the ImGui backend.
/// </summary>
public class ImGuiFontManager : IAbstFontManager
{
    public const string DefaultFontName = "default";
    private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<(string Name, AbstFontStyle Style), AbstImGuiFont> _loadedFonts = new();

    public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
    {
        _fontsToLoad.Add((name, style, pathAndName));
        return this;
    }

    public void LoadAll()
    {
        if (_loadedFonts.Count == 0)
        {
            var tahoma = Path.Combine(AppContext.BaseDirectory, "Fonts", "Tahoma.ttf");
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.Regular), new AbstImGuiFont(this, "Tahoma", tahoma));
            _loadedFonts.Add(("tahoma", AbstFontStyle.Regular), new AbstImGuiFont(this, "Tahoma", tahoma));
        }
        foreach (var font in _fontsToLoad)
        {
            var path = Path.IsPathRooted(font.FileName)
                ? font.FileName
                : Path.Combine(AppContext.BaseDirectory, font.FileName.Replace("\\", "/"));
            _loadedFonts[(font.Name.ToLower(), font.Style)] = new AbstImGuiFont(this, font.Name, path);
        }

        _fontsToLoad.Clear();
        InitFonts();
    }

    public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
    {
        if (string.IsNullOrEmpty(name))
            return _loadedFonts[(DefaultFontName, AbstFontStyle.Regular)] as T;
        var nameLower = name.ToLower();
        if (_loadedFonts.TryGetValue((nameLower, style), out var f))
            return f as T;
        if (_loadedFonts.TryGetValue((nameLower, AbstFontStyle.Regular), out f))
            return f as T;
        return _loadedFonts[(DefaultFontName, AbstFontStyle.Regular)] as T;
    }

    public IImGuiFontLoadedByUser GetTyped(object fontUser, string? name, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        if (string.IsNullOrEmpty(name))
            return _loadedFonts[(DefaultFontName, style)].Get(fontUser, fontSize);
        var nameLower = name.ToLower();
        if (_loadedFonts.TryGetValue((nameLower, style), out var f))
            return f.Get(fontUser, fontSize);
        if (_loadedFonts.TryGetValue((nameLower, AbstFontStyle.Regular), out f))
            return f.Get(fontUser, fontSize);
        return _loadedFonts[(DefaultFontName, style)].Get(fontUser, fontSize);
    }

    public T GetDefaultFont<T>() where T : class
        => _loadedFonts.TryGetValue((DefaultFontName, AbstFontStyle.Regular), out var f) ? (f as T)! : throw new KeyNotFoundException("Default font not found");

    public void SetDefaultFont<T>(T font) where T : class
    {
        if (font is not IAbstImGuiFont sdlFont)
            throw new ArgumentException("Font must be of type IAbstImGuiFont", nameof(font));
        _loadedFonts[(DefaultFontName, AbstFontStyle.Regular)] = (AbstImGuiFont)sdlFont;
    }

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    // ImGui fonts
    private readonly Dictionary<int, ImFontPtr> _fonts = new();

    public void InitFonts()
    {
        // Font initialization to be implemented.
    }

    public ImFontPtr? GetFont(int size)
        => _fonts.TryGetValue(size, out var font) ? font : null;

    private class AbstImGuiFont : IAbstImGuiFont
    {
        private Dictionary<int, LoadedFontWithSize> _loadedFontSizes = new();
        public string FileName { get; private set; }
        public string Name { get; private set; }

        internal AbstImGuiFont(ImGuiFontManager fontManager, string name, string fontFileName)
        {
            FileName = fontFileName;
            Name = name.Trim();
        }

        public IImGuiFontLoadedByUser Get(object fontUser, int fontSize)
        {
            if (!_loadedFontSizes.TryGetValue(fontSize, out var loadedFont))
            {
                loadedFont = new LoadedFontWithSize(FileName, fontSize, f => _loadedFontSizes.Remove(fontSize));
                _loadedFontSizes[fontSize] = loadedFont;
            }
            return loadedFont.AddUser(fontUser);
        }
    }

    private class LoadedFontWithSize
    {
        public nint FontHandle { get; set; }
        private Dictionary<object, ImGuiLoadedFontByUser> _fontUsers = new();
        private Action<LoadedFontWithSize> _onRemove;

        public LoadedFontWithSize(string fileName, int fontSize, Action<LoadedFontWithSize> onRemove)
        {
            _onRemove = onRemove;
            FontHandle = nint.Zero;
        }

        public IImGuiFontLoadedByUser AddUser(object user)
        {
            var subscription = new ImGuiLoadedFontByUser(FontHandle, f => RemoveUser(user));
            _fontUsers.Add(user, subscription);
            return subscription;
        }
        private void RemoveUser(object user)
        {
            _fontUsers.Remove(user);
            if (_fontUsers.Count == 0)
            {
                _onRemove(this);
                FontHandle = nint.Zero;
            }
        }
    }

    private class ImGuiLoadedFontByUser : IImGuiFontLoadedByUser
    {
        private readonly nint _fontHandle;
        private readonly Action<ImGuiLoadedFontByUser> _onRemove;

        public ImGuiLoadedFontByUser(nint fontHandle, Action<ImGuiLoadedFontByUser> onRemove)
        {
            _fontHandle = fontHandle;
            _onRemove = onRemove;
        }

        public nint FontHandle => _fontHandle;

        public void Release() => _onRemove(this);
    }
}
