using Godot;
using LingoEngine.Primitives;
using LingoEngine.Styles;
using LingoEngine.Texts;

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
        public static LabelSettings SetLingoColor(this LabelSettings labelSettings, LingoColor color)
        {
            labelSettings.FontColor = new Color(color.R, color.G, color.B);
            return labelSettings;
        }
        public static HorizontalAlignment ToGodot(this LingoTextAlignment alignment)
        {
            switch (alignment)
            {
                case LingoTextAlignment.Left: return HorizontalAlignment.Left;
                case LingoTextAlignment.Center: return HorizontalAlignment.Center;
                case LingoTextAlignment.Right: return HorizontalAlignment.Right;
                case LingoTextAlignment.Justified: return HorizontalAlignment.Fill;
                default: return HorizontalAlignment.Left;
            };
        } 
        public static LingoTextAlignment ToLingo(this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left: return LingoTextAlignment.Left;
                case HorizontalAlignment.Center: return LingoTextAlignment.Center;
                case HorizontalAlignment.Right: return LingoTextAlignment.Right;
                case HorizontalAlignment.Fill: return LingoTextAlignment.Justified;
                default: return LingoTextAlignment.Left;
            };
        }
        public static LingoColor GetLingoColor(this LabelSettings labelSettings)
            => new LingoColor(-1, (byte)labelSettings.FontColor.R, (byte)labelSettings.FontColor.G, (byte)labelSettings.FontColor.B);
    }
}
