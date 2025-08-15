namespace AbstUI.Primitives
{
    /// <summary>
    /// Simulates AbstUI's colorList: a predefined set of named system colors.
    /// Includes static properties for convenient access and a dictionary for lookup.
    /// </summary>
    public static class AColors
    {
        // Static predefined colors
        public static AColor Black => new(0, 0, 0, 0, "black");
        public static AColor White => new(255, 255, 255, 255, "white");
        public static AColor Red => new(1, 255, 0, 0, "red");
        public static AColor Green => new(2, 0, 255, 0, "green");
        public static AColor Blue => new(3, 0, 0, 255, "blue");
        public static AColor Yellow => new(4, 255, 255, 0, "yellow");
        public static AColor Cyan => new(5, 0, 255, 255, "cyan");
        public static AColor Magenta => new(6, 255, 0, 255, "magenta");
        public static AColor Gray => new(7, 128, 128, 128, "gray");
        public static AColor LightGray => new(8, 192, 192, 192, "lightgray");
        public static AColor DarkGray => new(9, 64, 64, 64, "darkgray");


        /// <summary>
        /// Dictionary of all predefined AbstUI colors by lowercase name.
        /// </summary>
        public static readonly Dictionary<string, AColor> Colors = new()
        {
            { "black", Black },
            { "white", White },
            { "red", Red },
            { "green", Green },
            { "blue", Blue },
            { "yellow", Yellow },
            { "cyan", Cyan },
            { "magenta", Magenta },
            { "gray", Gray },
            { "lightgray", LightGray },
            { "darkgray", DarkGray },
        };
        public static AColor Transparent = AColor.Transparent();

        /// <summary>
        /// Attempts to retrieve a color from the color list by name.
        /// </summary>
        public static bool TryGetColor(string name, out AColor color) =>
            Colors.TryGetValue(name.ToLowerInvariant(), out color);
    }
}



