using System.Collections.Generic;

namespace AbstUI.Styles
{
    public interface IAbstFontManager
    {
        IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular);
        void LoadAll();
        T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class;
        T GetDefaultFont<T>() where T : class;
        void SetDefaultFont<T>(T font) where T : class;
        IEnumerable<string> GetAllNames();

        float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);
        FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);

        /// <summary>
        /// Replaces the current set of font and character mappings with the provided data.
        /// </summary>
        void LoadFontMap(IEnumerable<AbstFontMap> fontMappings, IEnumerable<AbstInputKeyMap> inputKeyMappings);

        /// <summary>Font mappings parsed from Director font map definitions.</summary>
        IReadOnlyList<AbstFontMap> FontMappings { get; }

        /// <summary>Character translation tables parsed from Director font map definitions.</summary>
        IReadOnlyList<AbstInputKeyMap> InputKeyMappings { get; }
    }

    public readonly record struct FontInfo(int FontHeight, int TopIndentation);
}
