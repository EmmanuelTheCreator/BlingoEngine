using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class LingoMovieDTO
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Tempo { get; set; }
    public int FrameCount { get; set; }
    public int StageWidth { get; set; } = 640;
    public int StageHeight { get; set; } = 480;
    public LingoColorDTO StageColor { get; set; } = new();
    public string About { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public List<LingoCastDTO> Casts { get; set; } = new();
    public List<LingoSpriteDTO> Sprites { get; set; } = new();
}
