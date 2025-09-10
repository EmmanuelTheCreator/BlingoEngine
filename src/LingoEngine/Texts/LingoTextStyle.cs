using AbstUI.Styles;

namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents Lingo text styles using bitmask values.
    /// You can combine styles using bitwise OR: Bold | Italic | Underline.
    /// </summary>
    [Flags]
    public enum LingoTextStyle
    {
        /// <summary>No style.</summary>
        None = 0,

        /// <summary>Bold text.</summary>
        Bold = 1,

        /// <summary>Italic text.</summary>
        Italic = 2,

        /// <summary>Underlined text.</summary>
        Underline = 4
    }

    public static class LingoTextStyleExtensions
    {
        public static AbstFontStyle ToAbstUI(this LingoTextStyle style)
        {
            AbstFontStyle result = AbstFontStyle.Regular;
            if (style.HasFlag(LingoTextStyle.Bold))
                result |= AbstFontStyle.Bold;
            if (style.HasFlag(LingoTextStyle.Italic))
                result |= AbstFontStyle.Italic;
            //if (style.HasFlag(LingoTextStyle.Underline))
            //    result |= AbstFontStyle.Underline;
            return result;
        }
        public static LingoTextStyle ToLingoTextStyle(this AbstFontStyle style)
        {
            LingoTextStyle result = LingoTextStyle.None;
            if (style.HasFlag(AbstFontStyle.Bold))
                result |= LingoTextStyle.Bold;
            if (style.HasFlag(AbstFontStyle.Italic))
                result |= LingoTextStyle.Italic;
            // Underline is not represented in AbstFontStyle
            return result;
        }
    }

}



