using Godot;

namespace AbstUI.LGodot.Styles
{
    public enum AbstGodotThemeElementType
    {
        Tabs,
        TabItem,
        PopupWindow
    }

    public interface IAbstGodotStyleManager
    {
        Theme? GetTheme(AbstGodotThemeElementType type);
        void Register(AbstGodotThemeElementType type, Theme theme);
    }

    internal class AbstGodotStyleManager : IAbstGodotStyleManager
    {
        private static readonly Dictionary<AbstGodotThemeElementType, Theme> _themes = new Dictionary<AbstGodotThemeElementType, Theme>();
        public Theme? GetTheme(AbstGodotThemeElementType type)
             => _themes.TryGetValue(type, out var theme) ? theme : null;

        public void Register(AbstGodotThemeElementType type, Theme theme)
        {
            if (_themes.ContainsKey(type))
            {
                _themes[type] = theme;
            }
            else
            {
                _themes.Add(type, theme);
            }
        }
    }
}
