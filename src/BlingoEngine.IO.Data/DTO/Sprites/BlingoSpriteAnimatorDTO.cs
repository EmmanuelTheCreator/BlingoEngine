using System.Collections.Generic;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public class BlingoSpriteAnimatorDTO
{
    public List<BlingoPointKeyFrameDTO> Position { get; set; } = new();
    public BlingoTweenOptionsDTO PositionOptions { get; set; } = new();

    public List<BlingoFloatKeyFrameDTO> Rotation { get; set; } = new();
    public BlingoTweenOptionsDTO RotationOptions { get; set; } = new();

    public List<BlingoFloatKeyFrameDTO> Skew { get; set; } = new();
    public BlingoTweenOptionsDTO SkewOptions { get; set; } = new();

    public List<BlingoColorKeyFrameDTO> ForegroundColor { get; set; } = new();
    public BlingoTweenOptionsDTO ForegroundColorOptions { get; set; } = new();

    public List<BlingoColorKeyFrameDTO> BackgroundColor { get; set; } = new();
    public BlingoTweenOptionsDTO BackgroundColorOptions { get; set; } = new();

    public List<BlingoFloatKeyFrameDTO> Blend { get; set; } = new();
    public BlingoTweenOptionsDTO BlendOptions { get; set; } = new();
}

