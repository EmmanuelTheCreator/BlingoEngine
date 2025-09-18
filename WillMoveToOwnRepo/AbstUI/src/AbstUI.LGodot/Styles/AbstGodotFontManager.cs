using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Styles;
using Godot;

namespace AbstUI.LGodot.Styles
{
    public class AbstGodotFontManager : AbstFontManagerBase
    {
        public static bool IsRunningInTest { get; set; }
        private readonly Dictionary<(string FontName, int Size), Dictionary<long, Image>> _atlasCache = new();

        private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fontsToLoad = new();
        private readonly Dictionary<(string Name, AbstFontStyle Style), FontFile> _loadedFonts = new();

        private Font _defaultStyle = ThemeDB.FallbackFont; //!IsRunningInTest? ThemeDB.FallbackFont : null!;

        public AbstGodotFontManager()
        {

        }

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
            foreach (var fontTriple in _fontsToLoad)
            {
                var fontFile = GD.Load<FontFile>($"res://{fontTriple.FileName}");
                if (fontFile == null)
                    throw new Exception("Font file not found:" + fontTriple.Name + ":" + fontTriple.FileName);

                TextServer.FontStyle fontStyle1 = fontTriple.Style switch
                {
                    AbstFontStyle.Italic => TextServer.FontStyle.Italic,
                    AbstFontStyle.Bold => TextServer.FontStyle.Bold,
                    AbstFontStyle.BoldItalic => TextServer.FontStyle.Bold | TextServer.FontStyle.Italic,
                    _ => 0,
                };
                fontFile.FontStyle = fontStyle1;
                _loadedFonts[(fontTriple.Name, fontTriple.Style)] = fontFile;
            }
            _fontsToLoad.Clear();
        }
        public override T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class
        {
            if (string.IsNullOrEmpty(name))
                return _defaultStyle as T;
            if (_loadedFonts.TryGetValue((name, style), out var fontt))
                return fontt as T;
            if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out fontt))
                return fontt as T;
            return _defaultStyle as T;
        }

        public FontFile GetTyped(string name, AbstFontStyle style = AbstFontStyle.Regular)
        {
            if (_loadedFonts.TryGetValue((name, style), out var fontt))
                return fontt;
            if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out fontt))
                return fontt;
            return (FontFile)_defaultStyle;
        }
        public FontFile? GetTypedOrDefault(string name, AbstFontStyle style = AbstFontStyle.Regular)
        {
            if (_loadedFonts.TryGetValue((name, style), out var fontt))
                return fontt;
            if (_loadedFonts.TryGetValue((name, AbstFontStyle.Regular), out fontt))
                return fontt;
            return (FontFile)_defaultStyle;
        }

        public override T GetDefaultFont<T>() where T : class => (_defaultStyle as T)!;
        public override void SetDefaultFont<T>(T font) where T : class => _defaultStyle = (font as Font)!;

        public override IEnumerable<string> GetAllNames() => _loadedFonts.Keys.Select(k => k.Name).Distinct();

        public override float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        {
            var font = string.IsNullOrEmpty(fontName)
                ? _defaultStyle
                : (_loadedFonts.TryGetValue((fontName, style), out var f) ? f :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out f) ? f : _defaultStyle));
            return font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize).X;
        }

        public override FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
        {
            var font = string.IsNullOrEmpty(fontName)
                ? _defaultStyle
                : (_loadedFonts.TryGetValue((fontName, style), out var f) ? f :
                (_loadedFonts.TryGetValue((fontName, AbstFontStyle.Regular), out f) ? f : _defaultStyle));
            int height = (int)font.GetHeight(fontSize);
            int ascent = (int)font.GetAscent(fontSize);
            return new FontInfo(height, ascent);
        }

        public Dictionary<long, Image> GetAtlasCache(string fontName, int fontSize)
        {
            if (!_atlasCache.TryGetValue((fontName, fontSize), out var atlasCache))
            {
                atlasCache = new Dictionary<long, Image>();
                _atlasCache[(fontName, fontSize)] = atlasCache;
            }
            return atlasCache;
        }

    }
}

