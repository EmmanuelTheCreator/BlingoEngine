using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.IO.Data.DTO.FilmLoops;

public class BlingoMemberFilmLoopDTO : BlingoMemberDTO
{
    public BlingoFilmLoopFramingDTO Framing { get; set; }
    public bool Loop { get; set; }
    public int FrameCount { get; set; }
    public List<BlingoFilmLoopMemberSpriteDTO> SpriteEntries { get; set; } = new();
    public List<BlingoFilmLoopSoundEntryDTO> SoundEntries { get; set; } = new();
}

