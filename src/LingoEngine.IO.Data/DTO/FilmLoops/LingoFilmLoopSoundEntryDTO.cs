using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.IO.Data.DTO.FilmLoops;

public class LingoFilmLoopSoundEntryDTO
{
    public int Channel { get; set; }
    public int StartFrame { get; set; }
    public LingoMemberRefDTO Member { get; set; } = new();
}
