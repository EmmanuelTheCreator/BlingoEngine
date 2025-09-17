namespace BlingoEngine.IO.Data.DTO.Sprites;

public enum BlingoTempoSpriteActionDTO
{
    ChangeTempo,
    WaitSeconds,
    WaitForUserInput,
    WaitForCuePoint
}

public class BlingoTempoSpriteDTO : BlingoSpriteBaseDTO
{
    public BlingoTempoSpriteActionDTO Action { get; set; }
    public int Tempo { get; set; }
    public float WaitSeconds { get; set; }
    public int CueChannel { get; set; }
    public int CuePoint { get; set; }
}

