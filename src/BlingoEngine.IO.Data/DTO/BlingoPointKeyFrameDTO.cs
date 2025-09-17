namespace BlingoEngine.IO.Data.DTO;

public class BlingoPointKeyFrameDTO
{
    public int Frame { get; set; }
    public BlingoPointDTO Value { get; set; }
    public BlingoEaseTypeDTO Ease { get; set; }
}

