using System.Runtime.CompilerServices;

namespace LingoEngine.Director.Core.Tools
{
    public class MathL
    {
        //
        // Summary:
        //     Rounds s to the nearest whole number. This is the same as Godot.Mathf.Round(System.Single),
        //     but returns an int.
        //
        // Parameters:
        //   s:
        //     The number to round.
        //
        // Returns:
        //     The rounded number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(float s)
        {
            return (int)MathF.Round(s);
        }
    }
}
