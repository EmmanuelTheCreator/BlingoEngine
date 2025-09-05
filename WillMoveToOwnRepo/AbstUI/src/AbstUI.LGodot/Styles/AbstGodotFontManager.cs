using AbstUI.Styles;
using Godot;


namespace AbstUI.LGodot.Styles
{
    public class AbstGodotFontManager : IAbstFontManager
    {
        public static bool IsRunningInTest { get; set; }
        private readonly Dictionary<(string FontName, int Size), Dictionary<long, Image>> _atlasCache = new();

        private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
        private readonly Dictionary<(string Name, AbstFontStyle Style), FontFile> _loadedFonts = new();

        private Font _defaultStyle = ThemeDB.FallbackFont; //!IsRunningInTest? ThemeDB.FallbackFont : null!;

        public AbstGodotFontManager()
        {

        }

        public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
        {
            _fontsToLoad.Add((name, style, pathAndName));
            return this;
        }
        public void LoadAll()
        {
            foreach (var font in _fontsToLoad)
            {
                var fontFile = GD.Load<FontFile>($"res://{font.FileName}");
                if (fontFile == null)
                    throw new Exception("Font file not found:" + font.Name + ":" + font.FileName);
                _loadedFonts[(font.Name, font.Style)] = fontFile;
            }
            _fontsToLoad.Clear();
        }
        public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
        {
            return _loadedFonts.TryGetValue((name, style), out FontFile? fontt) ? fontt as T : null;
        }

        public FontFile GetTyped(string name, AbstFontStyle style = AbstFontStyle.Regular)
            => _loadedFonts[(name, style)];
        public FontFile? GetTypedOrDefault(string name, AbstFontStyle style = AbstFontStyle.Regular)
        {
            return _loadedFonts.TryGetValue((name, style), out var fontt) ? fontt : (FontFile)_defaultStyle;
        }

        
        public T GetDefaultFont<T>() where T : class => (_defaultStyle as T)!;
        public void SetDefaultFont<T>(T font) where T : class => _defaultStyle = (font as Font)!;

        public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

        public float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        {
            var font = string.IsNullOrEmpty(fontName)
                ? _defaultStyle
                : (_loadedFonts.TryGetValue((fontName, style), out var f) ? f : _defaultStyle);
            return font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize).X;
        }

        public FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        {
            var font = string.IsNullOrEmpty(fontName)
                ? _defaultStyle
                : (_loadedFonts.TryGetValue((fontName, style), out var f) ? f : _defaultStyle);
            int height = (int)font.GetHeight(fontSize);
            int ascent = (int)font.GetAscent(fontSize);
            return new FontInfo(height, ascent);
        }

        public Dictionary<long, Image> GetAtlasCache(string fontName, int fontSize)
        {
            if (!_atlasCache.TryGetValue((fontName, fontSize), out var atlasCache))
            {
                atlasCache = new Dictionary<long, Image>();
                _atlasCache[(fontName , fontSize)] = atlasCache;
            }
            return atlasCache;
        }
    }
}
