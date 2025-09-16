namespace LingoEngine.IO.Data.DTO;

public class LingoStageDTO
{
   
    public float Width { get; set; } = 640;
    public float Height { get; set; } = 480;
    public LingoColorDTO BackgroundColor { get; set; } = new LingoColorDTO { Name = "black" ,A = 255};
}
