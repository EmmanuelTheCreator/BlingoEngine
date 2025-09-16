namespace LingoEngine.IO.Data.DTO.Sprites;

public enum LingoTempoSpriteActionDTO
{
    ChangeTempo,
    WaitSeconds,
    WaitForUserInput,
    WaitForCuePoint
}

public class LingoTempoSpriteDTO : LingoSpriteBaseDTO
{
    public int Frame { get; set; }
    public LingoTempoSpriteActionDTO Action { get; set; }
    public int Tempo { get; set; }
    public float WaitSeconds { get; set; }
    public int CueChannel { get; set; }
    public int CuePoint { get; set; }
}
