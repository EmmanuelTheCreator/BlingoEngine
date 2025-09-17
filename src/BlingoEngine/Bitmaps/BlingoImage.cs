namespace BlingoEngine.Bitmaps
{
    /// <summary>
    /// Lingo Image interface.
    /// </summary>
    public interface IBlingoImage
    {
        public int Width { get; }
        public int Height { get; }
        /// <summary>
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        public int BitDepths { get; }
    }
    public class BlingoImage : IBlingoImage
    {
        public static int[] AllowedBitDepthsValues => new[] { 1, 2, 4, 8, 16, 32 };
        public int Width { get; private set; }

        public int Height { get; private set; }

        public int BitDepths { get; private set; }

        public BlingoImage(int width, int height, int bitDepths)
        {
            if (!AllowedBitDepthsValues.Contains(bitDepths))
                throw new ArgumentException("bitDepths must be in the range of " + string.Join(",", AllowedBitDepthsValues), nameof(BitDepths));
            Width = width;
            Height = height;
            BitDepths = bitDepths;
        }
    }
}

