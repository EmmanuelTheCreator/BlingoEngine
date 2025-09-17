using AbstUI.Primitives;

namespace BlingoEngine.L3D.Core.Members;

/// <summary>
/// Texture applied to a model surface.
/// </summary>
public class BlingoTexture
{
    public string FileName { get; set; } = string.Empty;
    public AColor? TransparentColor { get; set; }
}

