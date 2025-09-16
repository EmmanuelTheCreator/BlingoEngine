namespace LingoEngine.IO.Data.DTO.Members;

public class LingoMemberRefDTO
{
    public int MemberNum { get; set; }
    public int CastLibNum { get; set; }

    public LingoMemberRefDTO(int memberNum, int castLibNum)
    {
        MemberNum = memberNum;
        CastLibNum = castLibNum;
    }
    public LingoMemberRefDTO()
    {
        
    }
}
