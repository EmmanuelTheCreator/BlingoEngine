namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Enumerates the four-character codec tags stored at bytes 0x08-0x0B of the movie header.
/// </summary>
public enum BlRifxCodec
{
    Unknown = 0,
    MV93,
    MC95,
    APPL,
    FGDM,
    FGDC
}
