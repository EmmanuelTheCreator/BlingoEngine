using LingoEngine.IO.Data.DTO;
using LingoEngine.Members;

namespace LingoEngine.IO;

internal static class MemberRefDtoConverter
{
    public static LingoMemberRefDTO? ToDto(ILingoMember? member)
    {
        if (member == null)
        {
            return null;
        }

        return new LingoMemberRefDTO
        {
            MemberNum = member.NumberInCast,
            CastNum = member.CastLibNum
        };
    }

    public static LingoMemberRefDTO? ToDto(int memberNum, int castNum)
    {
        if (memberNum <= 0 || castNum <= 0)
        {
            return null;
        }

        return new LingoMemberRefDTO
        {
            MemberNum = memberNum,
            CastNum = castNum
        };
    }
}
