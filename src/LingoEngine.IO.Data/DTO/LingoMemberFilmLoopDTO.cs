namespace LingoEngine.IO.Data.DTO;

public class LingoMemberFilmLoopDTO : LingoMemberDTO
{
    public LingoFilmLoopFramingDTO Framing { get; set; }
    public bool Loop { get; set; }
    public int FrameCount { get; set; }
    public List<LingoFilmLoopMemberSpriteDTO> SpriteEntries { get; set; } = new();
    public List<LingoFilmLoopSoundEntryDTO> SoundEntries { get; set; } = new();
}
