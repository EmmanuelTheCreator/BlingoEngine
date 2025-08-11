using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class LingoFilmLoopDTO
{
    public int FrameCount { get; set; }
    public List<LingoFilmLoopSpriteEntryDTO> SpriteEntries { get; set; } = new();
    public List<LingoFilmLoopSoundEntryDTO> SoundEntries { get; set; } = new();
}
