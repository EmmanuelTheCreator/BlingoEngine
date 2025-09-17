using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.IO.Data.DTO;
public class BlingoMovieDTO
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Tempo { get; set; }
    public int FrameCount { get; set; }
    public string About { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public List<BlingoCastDTO> Casts { get; set; } = new();
    public List<Blingo2DSpriteDTO> Sprite2Ds { get; set; } = new();
    public List<BlingoTempoSpriteDTO> TempoSprites { get; set; } = new();
    public List<BlingoColorPaletteSpriteDTO> ColorPaletteSprites { get; set; } = new();
    public List<BlingoTransitionSpriteDTO> TransitionSprites { get; set; } = new();
    public List<BlingoSpriteSoundDTO> SoundSprites { get; set; } = new();
    public string UserName { get; set; } =string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of sprite channels in the movie.
    /// Lingo: lastChannel
    /// </summary>
    public int MaxSpriteChannelCount { get; set; }


}

