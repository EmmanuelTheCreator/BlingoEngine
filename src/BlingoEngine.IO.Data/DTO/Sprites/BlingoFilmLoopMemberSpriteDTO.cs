using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public class BlingoFilmLoopMemberSpriteDTO
{
    public string Name { get; set; } = "";
    public BlingoMemberRefDTO Member { get; set; } = new();
    public int DisplayMember { get; set; }
    public int SpriteNum { get; set; }

    public int Channel { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }

    public int Ink { get; set; }

    public bool Hilite { get; set; }
    public float Blend { get; set; } = 100f;
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }

    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }

    public BlingoPointDTO RegPoint { get; set; }
    public BlingoColorDTO ForeColor { get; set; }
    public BlingoColorDTO BackColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public BlingoSpriteAnimatorDTO Animator { get; set; } = new();

}

