namespace AbstUI.Styles
{
    public interface IAbstUIFontManager
    {
        IAbstUIFontManager AddFont(string name, string pathAndName);
        void LoadAll();
        T? Get<T>(string name) where T : class;
        T GetDefaultFont<T>() where T : class;
        void SetDefaultFont<T>(T font) where T : class;
        IEnumerable<string> GetAllNames();
    }
}
