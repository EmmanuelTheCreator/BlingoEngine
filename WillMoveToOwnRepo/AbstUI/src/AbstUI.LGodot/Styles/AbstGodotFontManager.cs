using AbstUI.Styles;
using Godot;
using System.Linq;

namespace AbstUI.LGodot.Styles
{
    public class AbstGodotFontManager : IAbstFontManager
    {
        private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
        private readonly Dictionary<(string Name, AbstFontStyle Style), FontFile> _loadedFonts = new();
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
             => _loadedFonts.TryGetValue((name, style), out var fontt) ? fontt as T : null;
        public FontFile GetTyped(string name, AbstFontStyle style = AbstFontStyle.Regular)
            => _loadedFonts[(name, style)];

        private Font _defaultStyle = ThemeDB.FallbackFont;
        public T GetDefaultFont<T>() where T : class => (_defaultStyle as T)!;
        public void SetDefaultFont<T>(T font) where T : class => _defaultStyle = (font as Font)!;

        public IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

        public float MeasureTextWidth(string text, string fontName, int fontSize)
        {
            var font = string.IsNullOrEmpty(fontName) ? _defaultStyle :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out var f) ? f : _defaultStyle);
            return font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize).X;
        }

        public FontInfo GetFontInfo(string fontName, int fontSize)
        {
            var font = string.IsNullOrEmpty(fontName) ? _defaultStyle :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out var f) ? f : _defaultStyle);
            int height = (int)font.GetHeight(fontSize);
            int ascent = (int)font.GetAscent(fontSize);
            return new FontInfo(height, height - ascent);
        }
    }
}
