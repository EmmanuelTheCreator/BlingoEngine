namespace BlingoEngine.IO.Data.DTO;

public class BlingoProjectDTO
{
    public List<BlingoMovieDTO> Movies { get; set; } = new();
    public BlingoStageDTO Stage { get; set; } = new();
}

