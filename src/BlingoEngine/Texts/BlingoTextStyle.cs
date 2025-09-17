using AbstUI.Styles;

namespace BlingoEngine.Texts
{
    /// <summary>
    /// Represents Lingo text styles using bitmask values.
    /// You can combine styles using bitwise OR: Bold | Italic | Underline.
    /// </summary>
    [Flags]
    public enum BlingoTextStyle
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

    public static class BlingoTextStyleExtensions
    {
        public static AbstFontStyle ToAbstUI(this BlingoTextStyle style)
        {
            AbstFontStyle result = AbstFontStyle.Regular;
            if (style.HasFlag(BlingoTextStyle.Bold))
                result |= AbstFontStyle.Bold;
            if (style.HasFlag(BlingoTextStyle.Italic))
                result |= AbstFontStyle.Italic;
            //if (style.HasFlag(BlingoTextStyle.Underline))
            //    result |= AbstFontStyle.Underline;
            return result;
        }
        public static BlingoTextStyle ToBlingoTextStyle(this AbstFontStyle style)
        {
            BlingoTextStyle result = BlingoTextStyle.None;
            if (style.HasFlag(AbstFontStyle.Bold))
                result |= BlingoTextStyle.Bold;
            if (style.HasFlag(AbstFontStyle.Italic))
                result |= BlingoTextStyle.Italic;
            // Underline is not represented in AbstFontStyle
            return result;
        }
    }

}




