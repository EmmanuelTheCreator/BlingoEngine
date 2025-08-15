using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// Light used to illuminate a 3D world.
/// </summary>
public class LingoLight
{
    public float Attenuation { get; set; } = 1f;
    public AColor Color { get; set; } = new();
    public AColor Specular { get; set; } = new();
    public float SpotAngle { get; set; } = 0f;
    public float SpotDecay { get; set; } = 0f;
    public string Type { get; set; } = "#point"; // #point, #directional, etc.
}
