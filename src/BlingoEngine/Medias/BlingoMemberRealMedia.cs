using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace BlingoEngine.Medias
{
    /// <summary>
    /// Represents a RealMedia digital video cast member.
    /// </summary>
    public class BlingoMemberRealMedia : BlingoMemberMedia
    {
        public BlingoMemberRealMedia(IBlingoFrameworkMemberMedia frameworkMember, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, BlingoMemberType.RealMedia, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}

