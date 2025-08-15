using AbstUI.Texts;
using Godot;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.LGodot.Texts
{
    public static class LingoLabelExtensions
    {
        public static LabelSettings SetLingoFont(this LabelSettings labelSettings, IAbstFontManager fontManager, string fontName)
        {
            var font = fontManager.Get<FontFile>(fontName);
            if (font != null)
                labelSettings.Font = font;
            return labelSettings;
        }
        public static LabelSettings SetLingoFontSize(this LabelSettings labelSettings, int fontSize)
        {
            if (fontSize > 0)
                labelSettings.FontSize = fontSize;
            return labelSettings;
        }
        public static LabelSettings SetLingoColor(this LabelSettings labelSettings, AColor color)
        {
            labelSettings.FontColor = new Color(color.R, color.G, color.B);
            return labelSettings;
        }
        public static HorizontalAlignment ToGodot(this AbstTextAlignment alignment)
        {
            switch (alignment)
            {
                case AbstTextAlignment.Left: return HorizontalAlignment.Left;
                case AbstTextAlignment.Center: return HorizontalAlignment.Center;
                case AbstTextAlignment.Right: return HorizontalAlignment.Right;
                case AbstTextAlignment.Justified: return HorizontalAlignment.Fill;
                default: return HorizontalAlignment.Left;
            };
        }
        public static AbstTextAlignment ToLingo(this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left: return AbstTextAlignment.Left;
                case HorizontalAlignment.Center: return AbstTextAlignment.Center;
                case HorizontalAlignment.Right: return AbstTextAlignment.Right;
                case HorizontalAlignment.Fill: return AbstTextAlignment.Justified;
                default: return AbstTextAlignment.Left;
            };
        }
        public static AColor GetLingoColor(this LabelSettings labelSettings)
            => new AColor(-1, (byte)labelSettings.FontColor.R, (byte)labelSettings.FontColor.G, (byte)labelSettings.FontColor.B);
    }
}
