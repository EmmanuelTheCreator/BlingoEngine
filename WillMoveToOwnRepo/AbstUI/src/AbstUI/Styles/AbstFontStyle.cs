using System;

namespace AbstUI.Styles;

[Flags]
public enum AbstFontStyle
{
    Regular = 0,
    Bold = 1 << 0,
    Italic = 1 << 1,
    BoldItalic = Bold | Italic
}

