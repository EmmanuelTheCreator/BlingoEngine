namespace BlingoEngine.IO.Data.DTO;

public class BlingoTweenOptionsDTO
{
    public bool Enabled { get; set; }
    public float Curvature { get; set; }
    public bool ContinuousAtEndpoints { get; set; }
    public BlingoSpeedChangeTypeDTO SpeedChange { get; set; }
    public float EaseIn { get; set; }
    public float EaseOut { get; set; }
}

