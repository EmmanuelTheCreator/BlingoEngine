using AbstUI.Styles;
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
        IAbstGodotStyleManager Register(AbstGodotThemeElementType type, Theme theme);
    }

    public class AbstGodotStyleManager : AbstStyleManager, IAbstGodotStyleManager
    {
        private static readonly Dictionary<AbstGodotThemeElementType, Theme> _themes = new Dictionary<AbstGodotThemeElementType, Theme>();
        public Theme? GetTheme(AbstGodotThemeElementType type)
             => _themes.TryGetValue(type, out var theme) ? theme : null;

        public IAbstGodotStyleManager Register(AbstGodotThemeElementType type, Theme theme)
        {
            if (_themes.ContainsKey(type))
            {
                _themes[type] = theme;
            }
            else
            {
                _themes.Add(type, theme);
            }
            return this;
        }
    }
}
