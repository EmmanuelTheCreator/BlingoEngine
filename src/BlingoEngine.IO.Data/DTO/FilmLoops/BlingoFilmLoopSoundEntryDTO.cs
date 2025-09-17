using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.FilmLoops;

public class BlingoFilmLoopSoundEntryDTO
{
    public int Channel { get; set; }
    public int StartFrame { get; set; }
    public BlingoMemberRefDTO Member { get; set; } = new();
}

