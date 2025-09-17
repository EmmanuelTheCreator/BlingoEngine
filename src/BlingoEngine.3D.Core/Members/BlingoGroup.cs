using BlingoEngine.L3D.Core.Primitives;

namespace BlingoEngine.L3D.Core.Members;

/// <summary>
/// A basic node in a 3D world used to group objects.
/// </summary>
public class BlingoGroup
{
    public string Name { get; set; } = string.Empty;
    public BlingoGroup? Parent { get; set; }
    public BlingoVector3 WorldPosition { get; set; } = new(0, 0, 0);
}

