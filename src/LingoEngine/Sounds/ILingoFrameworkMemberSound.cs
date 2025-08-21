using LingoEngine.Members;

namespace LingoEngine.Sounds
{
    /// <summary>
    /// Lingo Framework Member Sound interface.
    /// </summary>
    public interface ILingoFrameworkMemberSound : ILingoFrameworkMember
    {
        bool Stereo { get; }
        double Length { get; }
    }
}
