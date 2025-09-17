namespace BlingoEngine.Director.Core.Icons
{
    public interface IBlingoIconSheet
    {
        int HorizontalSpacing { get; set; }
        int IconCount { get; set; }
        int IconHeight { get; set; }
        int IconWidth { get; set; }
    }
    public class BlingoIconSheet<TTexture> : IBlingoIconSheet
    {
        public TTexture Image { get; set; }
        public int IconWidth { get; set; }
        public int IconHeight { get; set; }
        public int HorizontalSpacing { get; set; }
        public int IconCount { get; set; }

        public BlingoIconSheet(TTexture image, int iconWidth, int iconHeight, int horizontalSpacing, int iconCount)
        {
            Image = image;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            HorizontalSpacing = horizontalSpacing;
            IconCount = iconCount;
        }
    }
}

