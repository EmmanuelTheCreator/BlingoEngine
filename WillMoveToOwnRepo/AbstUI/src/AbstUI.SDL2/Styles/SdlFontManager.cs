using AbstUI.SDL2.SDLL;
using AbstUI.Styles;
using System.IO;
using System.Linq;

namespace AbstUI.SDL2.Styles;

public interface ISdlFontLoadedByUser
{
    nint FontHandle { get; }
    string Name { get; }
    void Release();
}
public interface IAbstSdlFont
{
    ISdlFontLoadedByUser? Get(object fontUser, int fontSize);
}
public class SdlFontManager : IAbstFontManager
{
    public const string DefaultFontName = "default";
    private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<(string Name, AbstFontStyle Style), AbstSdlFont> _loadedFonts = new();
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
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.Regular), new AbstSdlFont(this, "Tahoma", tahoma));
            _loadedFonts.Add(("tahoma", AbstFontStyle.Regular), new AbstSdlFont(this, "Tahoma", tahoma));
        }

        foreach (var font in _fontsToLoad)
        {
            var path = Path.IsPathRooted(font.FileName)
                ? font.FileName
                : Path.Combine(AppContext.BaseDirectory, font.FileName.Replace("\\", "/"));
            _loadedFonts[(font.Name.ToLower(), font.Style)] = new AbstSdlFont(this, font.Name, path);
        }

        _fontsToLoad.Clear();
        InitFonts();

    }
    public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
    {
        if (string.IsNullOrEmpty(name)) return null;
        var nameLower = name.ToLower();
        if (_loadedFonts.ContainsKey((nameLower, style)))
            return _loadedFonts[(nameLower, style)] as T;
        if (_loadedFonts.ContainsKey((nameLower, AbstFontStyle.Regular)))
            return _loadedFonts[(nameLower, AbstFontStyle.Regular)] as T;
        return null;
    }

    public ISdlFontLoadedByUser GetTyped(object fontUser, string? name, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        if (string.IsNullOrEmpty(name)) return _loadedFonts[(DefaultFontName, style)].Get(fontUser, fontSize);
        var nameLower = name.ToLower();
        if (_loadedFonts.ContainsKey((nameLower, style)))
            return _loadedFonts[(nameLower, style)].Get(fontUser, fontSize);
        if (_loadedFonts.ContainsKey((nameLower, AbstFontStyle.Regular)))
            return _loadedFonts[(nameLower, AbstFontStyle.Regular)].Get(fontUser, fontSize);
        return _loadedFonts[(DefaultFontName, style)].Get(fontUser, fontSize);
    }

    public T GetDefaultFont<T>() where T : class
        => _loadedFonts.TryGetValue((DefaultFontName, AbstFontStyle.Regular), out var f) ? (f as T)! : throw new KeyNotFoundException("Default font not found");

    public void SetDefaultFont<T>(T font) where T : class
    {
        if (font is not IAbstSdlFont sdlFont)
            throw new ArgumentException("Font must be of type IAbstSdlFont", nameof(font));
        _loadedFonts[(DefaultFontName, AbstFontStyle.Regular)] = (AbstSdlFont)sdlFont;
    }

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var user = new object();
        var font = GetTyped(user, string.IsNullOrEmpty(fontName) ? null : fontName, fontSize, style);
        SDL_ttf.TTF_SizeUTF8(font.FontHandle, text, out int w, out _);
        font.Release();
        return w;
    }

    public FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var user = new object();
        var font = GetTyped(user, string.IsNullOrEmpty(fontName) ? null : fontName, fontSize, style);
        int height = SDL_ttf.TTF_FontHeight(font.FontHandle);
        int ascent = SDL_ttf.TTF_FontAscent(font.FontHandle);
        font.Release();
        return new FontInfo(height, ascent);
    }

    // SDL Fonts
    public void InitFonts()
    {
        // No additional fonts to initialize.
    }

    public object? GetFont(int size) => null;


    private class AbstSdlFont : IAbstSdlFont
    {
        private Dictionary<int, LoadedFontWithSize> _loadedFontSizes = new();
        public string FileName { get; private set; }
        public string Name { get; private set; }

        internal AbstSdlFont(SdlFontManager fontManager, string name, string fontFileName)
        {
            FileName = fontFileName;
            Name = name.Trim();
        }


        public ISdlFontLoadedByUser Get(object fontUser, int fontSize)
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


        private Dictionary<object, SdlLoadedFontByUser> _fontUsers = new();
        private Action<LoadedFontWithSize> _onRemove;

        public nint FontHandle { get; set; }
        public string Name { get; }
        public int Ascent { get; }
        public int Descent { get; }
        public int Kerning { get; }
        public int LineGap { get; }
        public int FontHeight { get; }

        public LoadedFontWithSize(string fileName, int fontSize, Action<LoadedFontWithSize> onRemove)
        {
            _onRemove = onRemove;
            FontHandle = SDL_ttf.TTF_OpenFont(fileName, fontSize);
            Name = Path.GetFileNameWithoutExtension(fileName) + $"_{fontSize}";
            Ascent = SDL_ttf.TTF_FontAscent(FontHandle);
            Descent = SDL_ttf.TTF_FontDescent(FontHandle);
            Kerning = SDL_ttf.TTF_GetFontKerning(FontHandle);
            LineGap = SDL_ttf.TTF_FontLineSkip(FontHandle);
            FontHeight = SDL_ttf.TTF_FontHeight(FontHandle);
        }

        public ISdlFontLoadedByUser AddUser(object user)
        {
            if (_fontUsers.ContainsKey(user)) return _fontUsers[user];
            var subscription = new SdlLoadedFontByUser(this, f => RemoveUser(user));
            _fontUsers.Add(user, subscription);
            return subscription;
        }
        private void RemoveUser(object user)
        {
            _fontUsers.Remove(user);
            if (_fontUsers.Count == 0)
            {
                _onRemove(this);
                // do not close font here, it will be closed in SdlFontManager
                //SDL_ttf.TTF_CloseFont(FontHandle);
                //FontHandle = nint.Zero;
            }
        }
    }

    private class SdlLoadedFontByUser : ISdlFontLoadedByUser
    {
        private readonly nint _fontHandle;
        private readonly Action<SdlLoadedFontByUser> _onRemove;
        public nint FontHandle => _fontHandle;
        public string Name { get; }
        //public int Ascent { get; }
        //public int Descent { get; }
        //public int Kerning { get; }
        //public int LineGap { get; }
        //public int FontHeight { get; }
        public SdlLoadedFontByUser(LoadedFontWithSize loadedFont, Action<SdlLoadedFontByUser> onRemove)
        {
            _fontHandle = loadedFont.FontHandle;
            Name = loadedFont.Name;
            //Ascent = loadedFont.Ascent;
            //Descent = loadedFont.Descent;
            //Kerning = loadedFont.Kerning;
            //LineGap = loadedFont.LineGap;
            //FontHeight = loadedFont.FontHeight;
            _onRemove = onRemove;
        }



        public void Release() => _onRemove(this);
    }

}
