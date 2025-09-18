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
public class SdlFontManager : AbstFontManagerBase
{
    public const string DefaultFontName = "default";
    private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<(string Name, AbstFontStyle Style), AbstSdlFont> _loadedFonts = new();
    public override IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
    {
        if (_fontsToLoad.Contains((name, style, pathAndName)))
            _fontsToLoad.Remove((name, style, pathAndName));
        _fontsToLoad.Add((name, style, pathAndName));
        // Add automatic all styles if Regular is added
        if (style == AbstFontStyle.Regular)
        {
            _fontsToLoad.Add((name, AbstFontStyle.Bold, pathAndName));
            _fontsToLoad.Add((name, AbstFontStyle.Italic, pathAndName));
            _fontsToLoad.Add((name, AbstFontStyle.BoldItalic, pathAndName));
        }
        return this;
    }
    public override void LoadAll()
    {
        if (_loadedFonts.Count == 0)
        {
            var tahoma = Path.Combine(AppContext.BaseDirectory, "Fonts", "Tahoma.ttf");
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.Regular), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Regular));
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.Bold), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Bold));
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.BoldItalic), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.BoldItalic));
            _loadedFonts.Add((DefaultFontName, AbstFontStyle.Italic), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Italic));
            _loadedFonts.Add(("tahoma", AbstFontStyle.Regular), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Regular));
            _loadedFonts.Add(("tahoma", AbstFontStyle.Bold), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Bold));
            _loadedFonts.Add(("tahoma", AbstFontStyle.Italic), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.Italic));
            _loadedFonts.Add(("tahoma", AbstFontStyle.BoldItalic), new AbstSdlFont(this, "Tahoma", tahoma, AbstFontStyle.BoldItalic));
        }

        foreach (var font in _fontsToLoad)
        {
            var path = Path.IsPathRooted(font.FileName)
                ? font.FileName
                : Path.Combine(AppContext.BaseDirectory, font.FileName.Replace("\\", "/"));
            _loadedFonts[(font.Name.ToLower(), font.Style)] = new AbstSdlFont(this, font.Name, path, font.Style);
        }

        _fontsToLoad.Clear();
        InitFonts();

    }

   
    public override T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
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

    public override T GetDefaultFont<T>() where T : class
        => _loadedFonts.TryGetValue((DefaultFontName, AbstFontStyle.Regular), out var f) ? (f as T)! : throw new KeyNotFoundException("Default font not found");

    public override void SetDefaultFont<T>(T font) where T : class
    {
        if (font is not IAbstSdlFont sdlFont)
            throw new ArgumentException("Font must be of type IAbstSdlFont", nameof(font));
        _loadedFonts[(DefaultFontName, AbstFontStyle.Regular)] = (AbstSdlFont)sdlFont;
    }

    public override IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

    public override float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var user = new object();
        var font = GetTyped(user, string.IsNullOrEmpty(fontName) ? null : fontName, fontSize, style);
        SDL_ttf.TTF_SizeUTF8(font.FontHandle, text, out int w, out _);
        font.Release();
        return w;
    }

    public override FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
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
        public AbstFontStyle Style { get; }
        public string Name { get; private set; }

        internal AbstSdlFont(SdlFontManager fontManager, string name, string fontFileName, AbstFontStyle style)
        {
            FileName = fontFileName;
            Style = style;
            Name = name.Trim();
        }


        public ISdlFontLoadedByUser Get(object fontUser, int fontSize)
        {
            if (!_loadedFontSizes.TryGetValue(fontSize, out var loadedFont))
            {
                loadedFont = new LoadedFontWithSize(FileName, fontSize, f => _loadedFontSizes.Remove(fontSize), Style);
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

        public LoadedFontWithSize(string fileName, int fontSize, Action<LoadedFontWithSize> onRemove, AbstFontStyle style)
        {
            _onRemove = onRemove;
            FontHandle = SDL_ttf.TTF_OpenFont(fileName, fontSize);
            if ((int)style > 0)
            {

            }
            var sdlFontStyle = style switch
            {
                AbstFontStyle.Regular => 0,
                AbstFontStyle.Bold => SDL_ttf.TTF_STYLE_BOLD,
                AbstFontStyle.Italic => SDL_ttf.TTF_STYLE_ITALIC,
                AbstFontStyle.BoldItalic => SDL_ttf.TTF_STYLE_BOLD | SDL_ttf.TTF_STYLE_ITALIC,
                _ => 0
            };
            SDL_ttf.TTF_SetFontStyle(FontHandle, sdlFontStyle);
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
