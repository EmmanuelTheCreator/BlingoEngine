namespace LingoEngine.IO.Data.DTO;

public enum LingoTransitionAffectsDTO
{
    EntireStage,
    ChangingAreaOnly,
    Custom
}

public class LingoTransitionFrameSettingsDTO
{
    public int TransitionId { get; set; }
    public string TransitionName { get; set; } = string.Empty;
    public float Duration { get; set; }
    public float Smoothness { get; set; }
    public LingoTransitionAffectsDTO Affects { get; set; }
    public LingoRectDTO Rect { get; set; } = new();
}

public class LingoTransitionSpriteDTO : LingoSpriteBaseDTO
{
    public LingoMemberRefDTO? Member { get; set; }
    public LingoTransitionFrameSettingsDTO? Settings { get; set; }
}
