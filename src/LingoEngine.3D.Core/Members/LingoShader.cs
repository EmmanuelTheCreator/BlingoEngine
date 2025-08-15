using AbstUI.Primitives;

namespace LingoEngine.L3D.Core.Members;

/// <summary>
/// Defines how a model surface is shaded.
/// </summary>
public class LingoShader
{
    public string Name { get; set; } = string.Empty;
    public AColor DiffuseColor { get; set; } = new();
    public AColor SpecularColor { get; set; } = new();
    public float Smoothness { get; set; } = 0f;
}
