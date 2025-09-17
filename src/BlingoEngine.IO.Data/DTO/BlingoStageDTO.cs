namespace BlingoEngine.IO.Data.DTO;

public class BlingoStageDTO
{
   
    public float Width { get; set; } = 640;
    public float Height { get; set; } = 480;
    public BlingoColorDTO BackgroundColor { get; set; } = new BlingoColorDTO { Name = "black" ,A = 255};
}

