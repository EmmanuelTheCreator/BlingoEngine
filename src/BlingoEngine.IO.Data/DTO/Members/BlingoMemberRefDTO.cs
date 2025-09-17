namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberRefDTO
{
    public int MemberNum { get; set; }
    public int CastLibNum { get; set; }

    public BlingoMemberRefDTO(int memberNum, int castLibNum)
    {
        MemberNum = memberNum;
        CastLibNum = castLibNum;
    }
    public BlingoMemberRefDTO()
    {
        
    }
}

