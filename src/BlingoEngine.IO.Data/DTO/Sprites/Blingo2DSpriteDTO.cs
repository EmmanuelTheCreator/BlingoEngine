using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public class Blingo2DSpriteDTO : BlingoSpriteBaseDTO
{
    public int SpriteNum { get; set; }
    public BlingoMemberRefDTO? Member { get; set; }
    public int DisplayMember { get; set; }
    public int SpritePropertiesOffset { get; set; }
    public bool Visibility { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public BlingoPointDTO RegPoint { get; set; }
    public int Ink { get; set; }
    public BlingoColorDTO ForeColor { get; set; }
    public BlingoColorDTO BackColor { get; set; }
    public float Blend { get; set; }
    public bool Editable { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int ScoreColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public BlingoSpriteAnimatorDTO? Animator { get; set; }
    public List<BlingoSpriteBehaviorDTO> Behaviors { get; set; } = new();
}

