using AbstUI.Primitives;

namespace BlingoEngine.L3D.Core.Primitives;

/// <summary>
/// Represents the start and end colors for particle systems.
/// </summary>
public struct BlingoColorRange
{
    public AColor Start { get; set; }
    public AColor End { get; set; }

    public BlingoColorRange(AColor start, AColor end)
    {
        Start = start;
        End = end;
    }
}

