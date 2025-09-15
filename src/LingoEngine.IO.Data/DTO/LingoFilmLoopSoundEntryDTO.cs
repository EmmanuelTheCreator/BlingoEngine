namespace LingoEngine.IO.Data.DTO;

public class LingoFilmLoopSoundEntryDTO
{
    public int Channel { get; set; }
    public int StartFrame { get; set; }
    public LingoMemberRefDTO Member { get; set; } = new();
}
