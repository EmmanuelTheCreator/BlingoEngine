namespace LingoEngine.IO.Data.DTO;

public class LingoSpriteSoundDTO : LingoSpriteBaseDTO
{
    public int Channel { get; set; }
    public LingoMemberRefDTO? Member { get; set; }
}
