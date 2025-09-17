using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public enum BlingoTransitionAffectsDTO
{
    EntireStage,
    ChangingAreaOnly,
    Custom
}

public class BlingoTransitionFrameSettingsDTO
{
    public int TransitionId { get; set; }
    public string TransitionName { get; set; } = string.Empty;
    public float Duration { get; set; }
    public float Smoothness { get; set; }
    public BlingoTransitionAffectsDTO Affects { get; set; }
    public BlingoRectDTO Rect { get; set; } = new();
}

public class BlingoTransitionSpriteDTO : BlingoSpriteBaseDTO
{
    public BlingoMemberRefDTO? Member { get; set; }
    public BlingoTransitionFrameSettingsDTO? Settings { get; set; }
}

