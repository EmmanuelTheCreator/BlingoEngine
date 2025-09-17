using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public class BlingoSpriteSoundDTO : BlingoSpriteBaseDTO
{
    public int Channel { get; set; }
    public BlingoMemberRefDTO? Member { get; set; }
}

