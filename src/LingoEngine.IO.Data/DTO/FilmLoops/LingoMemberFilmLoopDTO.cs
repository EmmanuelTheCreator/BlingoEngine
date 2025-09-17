using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.IO.Data.DTO.Sprites;

namespace LingoEngine.IO.Data.DTO.FilmLoops;

public class LingoMemberFilmLoopDTO : LingoMemberDTO
{
    public LingoFilmLoopFramingDTO Framing { get; set; }
    public bool Loop { get; set; }
    public int FrameCount { get; set; }
    public List<LingoFilmLoopMemberSpriteDTO> SpriteEntries { get; set; } = new();
    public List<LingoFilmLoopSoundEntryDTO> SoundEntries { get; set; } = new();
}
