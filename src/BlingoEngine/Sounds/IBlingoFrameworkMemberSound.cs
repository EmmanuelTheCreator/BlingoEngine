using BlingoEngine.Members;

namespace BlingoEngine.Sounds
{
    /// <summary>
    /// Lingo Framework Member Sound interface.
    /// </summary>
    public interface IBlingoFrameworkMemberSound : IBlingoFrameworkMember
    {
        bool Stereo { get; }
        double Length { get; }
    }
}

