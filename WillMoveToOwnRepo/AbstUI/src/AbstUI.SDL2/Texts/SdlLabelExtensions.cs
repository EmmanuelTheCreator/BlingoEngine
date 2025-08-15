using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.SDL2.Texts;

public static class SdlLabelExtensions
{
    public static object SetLingoFont(this object label, IAbstFontManager manager, string fontName) => label;
    public static object SetLingoFontSize(this object label, int size) => label;
    public static object SetLingoColor(this object label, AColor color) => label;
    public static AColor GetLingoColor(this object label) => AColor.FromRGB(0, 0, 0);
}
