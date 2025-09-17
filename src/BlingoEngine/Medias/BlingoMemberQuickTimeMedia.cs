using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace BlingoEngine.Medias
{
    /// <summary>
    /// Represents a QuickTime digital video cast member.
    /// </summary>
    public class BlingoMemberQuickTimeMedia : BlingoMemberMedia
    {
        public BlingoMemberQuickTimeMedia(IBlingoFrameworkMemberMedia frameworkMember, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, BlingoMemberType.QuickTimeMedia, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}

