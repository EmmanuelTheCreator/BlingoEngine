namespace LingoEngine.IO.Data.DTO;

public class LingoFilmLoopSpriteEntryDTO
{
    public int Channel { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public Lingo2DSpriteDTO Sprite { get; set; } = new();
}
