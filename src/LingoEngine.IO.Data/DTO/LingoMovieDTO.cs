using LingoEngine.IO.Data.DTO.Sprites;

namespace LingoEngine.IO.Data.DTO;
public class LingoMovieDTO
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Tempo { get; set; }
    public int FrameCount { get; set; }
    public string About { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public List<LingoCastDTO> Casts { get; set; } = new();
    public List<Lingo2DSpriteDTO> Sprite2Ds { get; set; } = new();
    public List<LingoTempoSpriteDTO> TempoSprites { get; set; } = new();
    public List<LingoColorPaletteSpriteDTO> ColorPaletteSprites { get; set; } = new();
    public List<LingoTransitionSpriteDTO> TransitionSprites { get; set; } = new();
    public List<LingoSpriteSoundDTO> SoundSprites { get; set; } = new();
    public string UserName { get; set; } =string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of sprite channels in the movie.
    /// Lingo: lastChannel
    /// </summary>
    public int MaxSpriteChannelCount { get; set; }


}
