using System.Collections.Generic;

namespace AbstUI.Styles;

/// <summary>
/// Base implementation of <see cref="IAbstFontManager"/> that manages font and input key mappings.
/// </summary>
public abstract class AbstFontManagerBase : IAbstFontManager
{
    private readonly List<AbstFontMap> _fontMappings = new();
    private readonly List<AbstInputKeyMap> _inputKeyMappings = new();

    public abstract IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular);

    public abstract void LoadAll();

    public abstract T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class;

    public abstract T GetDefaultFont<T>() where T : class;

    public abstract void SetDefaultFont<T>(T font) where T : class;

    public abstract IEnumerable<string> GetAllNames();

    public abstract float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);

    public abstract FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);

    public virtual void LoadFontMap(IEnumerable<AbstFontMap> fontMappings, IEnumerable<AbstInputKeyMap> inputKeyMappings)
    {
        _fontMappings.Clear();
        _inputKeyMappings.Clear();

        if (fontMappings != null)
        {
            foreach (var mapping in fontMappings)
                _fontMappings.Add(mapping);
        }

        if (inputKeyMappings != null)
        {
            foreach (var mapping in inputKeyMappings)
                _inputKeyMappings.Add(mapping);
        }
    }

    public IReadOnlyList<AbstFontMap> FontMappings => _fontMappings;

    public IReadOnlyList<AbstInputKeyMap> InputKeyMappings => _inputKeyMappings;
}
