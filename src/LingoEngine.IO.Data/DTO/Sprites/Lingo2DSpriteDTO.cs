using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.IO.Data.DTO.Sprites;

public class Lingo2DSpriteDTO : LingoSpriteBaseDTO
{
    public int SpriteNum { get; set; }
    public LingoMemberRefDTO? Member { get; set; }
    public int DisplayMember { get; set; }
    public int SpritePropertiesOffset { get; set; }
    public bool Visibility { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public LingoPointDTO RegPoint { get; set; }
    public int Ink { get; set; }
    public LingoColorDTO ForeColor { get; set; }
    public LingoColorDTO BackColor { get; set; }
    public float Blend { get; set; }
    public bool Editable { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int ScoreColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public LingoSpriteAnimatorDTO? Animator { get; set; }
}
