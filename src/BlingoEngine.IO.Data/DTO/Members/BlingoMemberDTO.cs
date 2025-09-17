namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberDTO
{
    public string Name { get; set; } = string.Empty;
    public int CastLibNum { get; set; }
    public int NumberInCast { get; set; }
    public BlingoMemberTypeDTO Type { get; set; }
    public BlingoPointDTO RegPoint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PurgePriority { get; set; }
}

