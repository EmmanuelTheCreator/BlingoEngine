namespace AbstUI.Styles
{
    public interface IAbstFontManager
    {
        IAbstFontManager AddFont(string name, string pathAndName);
        void LoadAll();
        T? Get<T>(string name) where T : class;
        T GetDefaultFont<T>() where T : class;
        void SetDefaultFont<T>(T font) where T : class;
        IEnumerable<string> GetAllNames();

        float MeasureTextWidth(string text, string fontName, int fontSize) => text.Length * fontSize * 0.6f;
        FontInfo GetFontInfo(string fontName, int fontSize) => new(fontSize, 0);
    }

    public readonly record struct FontInfo(int FontHeight, int TopIndentation);
}
