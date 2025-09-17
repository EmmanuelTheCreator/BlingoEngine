using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Members;

namespace BlingoEngine.IO;

internal static class MemberRefDtoConverter
{
    public static BlingoMemberRefDTO? ToDto(this IBlingoMember? member)
    {
        if (member == null)
        {
            return null;
        }

        return new BlingoMemberRefDTO
        {
            MemberNum = member.NumberInCast,
            CastLibNum = member.CastLibNum
        };
    }

    public static BlingoMemberRefDTO? ToDto(this int memberNum, int castNum)
    {
        if (memberNum <= 0 || castNum <= 0)
        {
            return null;
        }

        return new BlingoMemberRefDTO
        {
            MemberNum = memberNum,
            CastLibNum = castNum
        };
    }
}

