using AbstUI.Primitives;
using BlingoEngine.Primitives;

namespace BlingoEngine.L3D.Core.Primitives;

/// <summary>
/// Model resource primitive of type #particle.
/// Contains an emitter and appearance properties.
/// </summary>
public class BlingoParticle
{
    public BlingoEmitter Emitter { get; set; } = new();
    public BlingoColorRange ColorRange { get; set; } = new(new AColor(), new AColor());
    public int Lifetime { get; set; } = 0; // in milliseconds
}

