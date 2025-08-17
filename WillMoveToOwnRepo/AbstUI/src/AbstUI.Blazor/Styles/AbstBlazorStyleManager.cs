namespace AbstUI.Blazor.Styles;

public enum AbstBlazorThemeElementType
{
    Tabs,
    TabItem,
    PopupWindow
}

public interface IAbstBlazorStyleManager
{
    string? GetStyle(AbstBlazorThemeElementType type);
    void Register(AbstBlazorThemeElementType type, string cssClass);
}

internal class AbstBlazorStyleManager : IAbstBlazorStyleManager
{
    private static readonly Dictionary<AbstBlazorThemeElementType, string> _styles = new();

    public string? GetStyle(AbstBlazorThemeElementType type)
        => _styles.TryGetValue(type, out var style) ? style : null;

    public void Register(AbstBlazorThemeElementType type, string cssClass)
    {
        _styles[type] = cssClass;
    }
}
