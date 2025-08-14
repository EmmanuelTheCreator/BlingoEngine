using ImGuiNET;
using LingoEngine.SDL2.SDLL;
using LingoEngine.Styles;

namespace LingoEngine.SDL2.Styles;

public interface ISdlFontLoadedByUser
{
    nint FontHandle { get; }
    void Release();
}
public interface ILingoSdlFont
{
    ISdlFontLoadedByUser? Get(object fontUser, int fontSize);
}
public class SdlFontManager : ILingoFontManager
{
    private readonly List<(string Name, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<string, LingoSdlFont> _loadedFonts = new();
    public ILingoFontManager AddFont(string name, string pathAndName)
    {
        _fontsToLoad.Add((name, pathAndName));
        return this;
    }
    public void LoadAll()
    {
        if (_loadedFonts.Count == 0)
            _loadedFonts.Add("Tahoma", new LingoSdlFont(this, "Tahoma", "Fonts\\Tahoma.ttf")); // default font
        foreach (var font in _fontsToLoad)
            _loadedFonts[font.Name] = new LingoSdlFont(this, font.Name, font.FileName); // placeholder

        _fontsToLoad.Clear();
        InitFonts();

    }
    public T? Get<T>(string name) where T : class
        => _loadedFonts.TryGetValue(name, out var f) ? f as T : null;
    public ISdlFontLoadedByUser GetTyped(object fontUser, string? name, int fontSize)
    {
        if (string.IsNullOrEmpty(name)) return _loadedFonts["Tahoma"].Get(fontUser,fontSize);
        return _loadedFonts[name].Get(fontUser, fontSize);
    }

    public T GetDefaultFont<T>() where T : class
        => _loadedFonts.TryGetValue("default", out var f) ? (f as T)! : throw new KeyNotFoundException("Default font not found");

    public void SetDefaultFont<T>(T font) where T : class
    {
        if (font is not ILingoSdlFont sdlFont)
            throw new ArgumentException("Font must be of type ILingoSdlFont", nameof(font));
        _loadedFonts["default"] = (LingoSdlFont)sdlFont;
    }

    public IEnumerable<string> GetAllNames() => _loadedFonts.Keys;



    // SDL Fonts
    private readonly Dictionary<int, ImFontPtr> _fonts = new();

    public unsafe void InitFonts()
    {
        //var io = ImGui.GetIO();

        //foreach (int size in new[] { 7, 8, 9, 10, 11, 12, 14, 16, 20, 24, 32 }) // etc
        //{
        //    var config = ImGuiNative.ImFontConfig_ImFontConfig(); // allocate native config
        //    ImFontConfigPtr configPtr = new ImFontConfigPtr(config);
        //    configPtr.SizePixels = size;
        //    var font = io.Fonts.AddFontDefault(configPtr);
        //    _fonts[size] = font;
        //}
        //io.Fonts.Build();
    }

    // TODO : use subscription based, add fontUser
    public ImFontPtr? GetFont(int size)
        => _fonts.TryGetValue(size, out var font) ? font : null;


    private class LingoSdlFont : ILingoSdlFont
    {
        private Dictionary<int, LoadedFontWithSize> _loadedFontSizes = new();
        public string FileName { get; private set; }
        public string Name { get; private set; }

        internal LingoSdlFont(SdlFontManager fontManager, string name, string fontFileName)
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
        public nint FontHandle { get; set; }
        private Dictionary<object, SdlLoadedFontByUser> _fontUsers = new();
        private Action<LoadedFontWithSize> _onRemove;

        public LoadedFontWithSize(string fileName, int fontSize, Action<LoadedFontWithSize> onRemove)
        {
            _onRemove = onRemove;
            FontHandle = SDL_ttf.TTF_OpenFont(fileName, fontSize);
        }

        public ISdlFontLoadedByUser AddUser(object user)
        {
            var subscription = new SdlLoadedFontByUser(FontHandle, f => RemoveUser(user));
            _fontUsers.Add(user, subscription);
            return subscription;
        }
        private void RemoveUser(object user)
        {
            _fontUsers.Remove(user);
            if (_fontUsers.Count == 0)
            {
                _onRemove(this);
                SDL_ttf.TTF_CloseFont(FontHandle);
                FontHandle = nint.Zero;
            }
        }
    }

    private class SdlLoadedFontByUser : ISdlFontLoadedByUser
    {
        private readonly nint _fontHandle;
        private readonly Action<SdlLoadedFontByUser> _onRemove;

        public SdlLoadedFontByUser(nint fontHandle, Action<SdlLoadedFontByUser> onRemove)
        {
            _fontHandle = fontHandle;
            _onRemove = onRemove;
        }

        public nint FontHandle => _fontHandle;

        public void Release() => _onRemove(this);
    }

}
