namespace LingoEngine.IO.Data.DTO;

public class LingoMemberFilmLoopDTO : LingoMemberDTO
{
    public LingoFilmLoopFramingDTO Framing { get; set; }
    public bool Loop { get; set; }
    public LingoFilmLoopDTO FilmLoop { get; set; } = new();
}
