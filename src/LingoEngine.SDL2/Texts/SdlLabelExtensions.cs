using LingoEngine.AbstUI.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.SDL2.Texts;

public static class SdlLabelExtensions
{
    public static object SetLingoFont(this object label, ILingoFontManager manager, string fontName) => label;
    public static object SetLingoFontSize(this object label, int size) => label;
    public static object SetLingoColor(this object label, AColor color) => label;
    public static AColor GetLingoColor(this object label) => AColor.FromRGB(0, 0, 0);
}
