using LingoEngine.Casts;
using LingoEngine.Primitives;

namespace LingoEngine.Medias
{
    /// <summary>
    /// Represents a QuickTime digital video cast member.
    /// </summary>
    public class LingoMemberQuickTimeMedia : LingoMemberMedia
    {
        public LingoMemberQuickTimeMedia(ILingoFrameworkMemberMedia frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, LingoMemberType.QuickTimeMedia, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}
