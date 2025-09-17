using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.IO.Data.DTO.Sprites;

public class LingoSpriteSoundDTO : LingoSpriteBaseDTO
{
    public int Channel { get; set; }
    public LingoMemberRefDTO? Member { get; set; }
}
