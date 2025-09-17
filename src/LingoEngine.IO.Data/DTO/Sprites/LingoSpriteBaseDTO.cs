namespace LingoEngine.IO.Data.DTO.Sprites;

public abstract class LingoSpriteBaseDTO
{
    public string Name { get; set; } = string.Empty;
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public bool Lock { get; set; }
}
