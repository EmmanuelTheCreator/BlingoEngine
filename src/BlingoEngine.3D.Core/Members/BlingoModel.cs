using BlingoEngine.L3D.Core.Primitives;

namespace BlingoEngine.L3D.Core.Members;

/// <summary>
/// Visible object within a 3D world.
/// </summary>
public class BlingoModel
{
    public BlingoModelResource? ModelResource { get; set; }
    public BlingoShader? Shader { get; set; }
    public BlingoTexture? Texture { get; set; }
    public BlingoVector3 Position { get; set; } = new(0, 0, 0);
    public BlingoVector3 Rotation { get; set; } = new(0, 0, 0);
    public BlingoVector3 Scale { get; set; } = new(1, 1, 1);
}

