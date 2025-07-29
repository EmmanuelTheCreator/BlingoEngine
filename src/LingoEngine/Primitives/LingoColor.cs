namespace LingoEngine.Primitives
{
    /// <summary>
    /// Represents a color value in the Lingo system, supporting palette index, RGB, and optional name.
    /// Platform-agnostic.
    /// </summary>
    public struct LingoColor
    {
        public int Code { get; set; }           // Lingo palette index (0–255) or -1 for custom
        public string Name { get; set; }        // Optional name ("Red", etc.)
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;


        public LingoColor(byte r, byte g, byte b,byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            Name = "";
        }
        public LingoColor(int code, byte r, byte g, byte b, string name = "", byte a = 255)
        {
            Code = code;
            R = r;
            G = g;
            B = b;
            A = a;
            Name = name;
        }

        /// <summary>
        /// Creates a grayscale version of the color using luminance weighting.
        /// </summary>
        public LingoColor ToGrayscale()
        {
            byte gray = (byte)(R * 0.3 + G * 0.59 + B * 0.11);
            return new LingoColor(Code, gray, gray, gray, Name + " (gray)");
        }

        /// <summary>
        /// Creates a lighter version of the color by blending towards white.
        /// Factor should be between 0 (no change) and 1 (white).
        /// </summary>
        public LingoColor Lighten(float factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);
            byte r = (byte)(R + (255 - R) * factor);
            byte g = (byte)(G + (255 - G) * factor);
            byte b = (byte)(B + (255 - B) * factor);
            return new LingoColor(Code, r, g, b, Name,A);
        }
        /// <summary>
        /// Creates a darker version of the color by blending towards black.
        /// Factor should be between 0 (no change) and 1 (black).
        /// </summary>
        public LingoColor Darken(float factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);
            byte r = (byte)(R * (1f - factor));
            byte g = (byte)(G * (1f - factor));
            byte b = (byte)(B * (1f - factor));
            return new LingoColor(Code, r, g, b, Name, A);
        }
        /// <summary>
        /// Converts the RGB color to a hex string, e.g., "#FF0000". or "#FF8800FF" if alpha is included.
        /// </summary>
        public string ToHex() => A < 255? $"#{R:X2}{G:X2}{B:X2}{A:X2}" : $"#{R:X2}{G:X2}{B:X2}";

        public override string ToString() => !string.IsNullOrWhiteSpace(Name) ? Name : ToHex();

        /// <summary>
        /// Parses a hex string (e.g. "#FF8800" or "FF8800") into a LingoColor. Alpha channel is ignored if present.
        /// </summary>
        public static LingoColor FromHex(string hex, int code = -1, string name = "")
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6 && hex.Length != 8)
                throw new FormatException("Hex string must be 6 or 8 characters long");
            byte a = 255;
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            if (hex.Length == 8)
                a = Convert.ToByte(hex.Substring(6, 2), 16);
            return new LingoColor(code, r, g, b, name,a);
        }

        /// <summary>
        /// Creates a LingoColor from an RGB tuple.
        /// </summary>
        public static LingoColor FromRGB(byte r, byte g, byte b, int code = -1, string name = "", byte a = 255)
            => new LingoColor(code, r, g, b, name, a);
        public static LingoColor Transparent() => new LingoColor { A=0, Code = -1, R = 0, G = 0, B = 0, Name = "transparent" };
    }

}
