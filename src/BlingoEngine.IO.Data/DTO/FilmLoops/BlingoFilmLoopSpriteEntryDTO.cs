using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.IO.Data.DTO.FilmLoops;

public class BlingoFilmLoopSpriteEntryDTO
{
    public int Channel { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public Blingo2DSpriteDTO Sprite { get; set; } = new();
}

