using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.L3D.Core.Primitives;

/// <summary>
/// Represents the start and end colors for particle systems.
/// </summary>
public struct LingoColorRange
{
    public AColor Start { get; set; }
    public AColor End { get; set; }

    public LingoColorRange(AColor start, AColor end)
    {
        Start = start;
        End = end;
    }
}
