using Godot;
using AbstUI.Primitives;

namespace AbstUI.LGodot.Primitives
{
    public static class AbstColorExtensions
    {
        /// <summary>
        /// Converts a Godot Color to a AbstColor.
        /// </summary>
        public static AColor ToAbstColor(this Color color)
        {
            return AColor.FromRGB(
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255),
                -1, "",
                (byte)(color.A * 255)
            );
        }

        /// <summary>
        /// Converts a AbstColor to a Godot Color.
        /// </summary>
        public static Color ToGodotColor(this AColor color)
        {
            return new Color(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            );
        }
    }

}
