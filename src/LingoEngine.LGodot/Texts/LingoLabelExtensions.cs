using AbstUI.Texts;
using Godot;
using AbstUI.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.LGodot.Texts
{
    public static class LingoLabelExtensions
    {
        public static LabelSettings SetLingoFont(this LabelSettings labelSettings, ILingoFontManager fontManager, string fontName)
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
        public static HorizontalAlignment ToGodot(this AbstUITextAlignment alignment)
        {
            switch (alignment)
            {
                case AbstUITextAlignment.Left: return HorizontalAlignment.Left;
                case AbstUITextAlignment.Center: return HorizontalAlignment.Center;
                case AbstUITextAlignment.Right: return HorizontalAlignment.Right;
                case AbstUITextAlignment.Justified: return HorizontalAlignment.Fill;
                default: return HorizontalAlignment.Left;
            };
        } 
        public static AbstUITextAlignment ToLingo(this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left: return AbstUITextAlignment.Left;
                case HorizontalAlignment.Center: return AbstUITextAlignment.Center;
                case HorizontalAlignment.Right: return AbstUITextAlignment.Right;
                case HorizontalAlignment.Fill: return AbstUITextAlignment.Justified;
                default: return AbstUITextAlignment.Left;
            };
        }
        public static AColor GetLingoColor(this LabelSettings labelSettings)
            => new AColor(-1, (byte)labelSettings.FontColor.R, (byte)labelSettings.FontColor.G, (byte)labelSettings.FontColor.B);
    }
}
