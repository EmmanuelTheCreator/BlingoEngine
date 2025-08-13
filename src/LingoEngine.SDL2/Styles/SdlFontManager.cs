using ImGuiNET;
using LingoEngine.Styles;

namespace LingoEngine.SDL2.Styles;

public class SdlFontManager : ILingoFontManager
{
    private readonly List<(string Name, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<string, object> _loadedFonts = new();
    public ILingoFontManager AddFont(string name, string pathAndName)
    {
        _fontsToLoad.Add((name, pathAndName));
        return this;
    }
    public void LoadAll()
    {
        foreach (var font in _fontsToLoad)
            _loadedFonts[font.Name] = font.FileName; // placeholder
        _fontsToLoad.Clear();
        InitFonts();
    }
    public T? Get<T>(string name) where T : class
        => _loadedFonts.TryGetValue(name, out var f) ? f as T : null;
    public object GetTyped(string name) => _loadedFonts[name];

    public T GetDefaultFont<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public void SetDefaultFont<T>(T font) where T : class
    {
        throw new NotImplementedException();
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

    public ImFontPtr? GetFont(int size)
        => _fonts.TryGetValue(size, out var font) ? font : null;
}
