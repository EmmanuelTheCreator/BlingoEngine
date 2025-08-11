namespace LingoEngine.IO.Data.DTO;

public class LingoProjectDTO
{
    public List<LingoMovieDTO> Movies { get; set; } = new();
    public LingoStageDTO Stage { get; set; } = new();
}
