using LingoEngine.Casts;
using LingoEngine.Primitives;

namespace LingoEngine.Medias
{
    /// <summary>
    /// Represents a RealMedia digital video cast member.
    /// </summary>
    public class LingoMemberRealMedia : LingoMemberMedia
    {
        public LingoMemberRealMedia(ILingoFrameworkMemberMedia frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, LingoMemberType.RealMedia, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}
