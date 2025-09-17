namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberSoundDTO : BlingoMemberDTO
{
    public bool Stereo { get; set; }
    public double Length { get; set; }
    public bool Loop { get; set; }
    public bool IsLinked { get; set; }
    public string LinkedFilePath { get; set; } = string.Empty;
    public string? SoundFile { get; set; }
}

