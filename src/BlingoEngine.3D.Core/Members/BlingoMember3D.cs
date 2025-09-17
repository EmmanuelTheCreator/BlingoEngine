using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace BlingoEngine.L3D.Core.Members;

/// <summary>
/// Represents a Shockwave 3D cast member containing a complete 3D world.
/// Mirrors the Lingo 3D Member object.
/// </summary>
public class BlingoMember3D : BlingoMember
{
    public List<BlingoCamera> Cameras { get; } = new();
    public List<BlingoGroup> Groups { get; } = new();
    public List<BlingoLight> Lights { get; } = new();
    public List<BlingoModel> Models { get; } = new();

    public BlingoMember3D(BlingoCast cast, IBlingoFrameworkMember3D frameworkMember, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
        : base(frameworkMember, BlingoMemberType.Shockwave3D, cast, numberInCast, name, fileName, regPoint)
    {
    }
}

