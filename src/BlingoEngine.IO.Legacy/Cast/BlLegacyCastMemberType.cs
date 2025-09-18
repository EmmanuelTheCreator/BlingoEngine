namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Enumerates the legacy cast-member types encoded at the start of the <c>CASt</c> payload.
/// </summary>
public enum BlLegacyCastMemberType
{
    Unknown = -1,
    Null = 0,
    Bitmap = 1,
    FilmLoop = 2,
    Text = 3,
    Palette = 4,
    Picture = 5,
    Sound = 6,
    Button = 7,
    Shape = 8,
    Movie = 9,
    DigitalVideo = 10,
    Script = 11,
    Rte = 12,
    Font = 13,
    Xtra = 14,
    Field = 15
}


public static class BlLegacyCastMemberTypeHelpers
{
    public static BlLegacyCastMemberType MapMemberType(uint value)
    {
        return value switch
        {
            0 => BlLegacyCastMemberType.Null,
            1 => BlLegacyCastMemberType.Bitmap,
            2 => BlLegacyCastMemberType.FilmLoop,
            3 => BlLegacyCastMemberType.Text,
            4 => BlLegacyCastMemberType.Palette,
            5 => BlLegacyCastMemberType.Picture,
            6 => BlLegacyCastMemberType.Sound,
            7 => BlLegacyCastMemberType.Button,
            8 => BlLegacyCastMemberType.Shape,
            9 => BlLegacyCastMemberType.Movie,
            10 => BlLegacyCastMemberType.DigitalVideo,
            11 => BlLegacyCastMemberType.Script,
            12 => BlLegacyCastMemberType.Rte,
            13 => BlLegacyCastMemberType.Font,
            14 => BlLegacyCastMemberType.Xtra,
            15 => BlLegacyCastMemberType.Field,
            _ => BlLegacyCastMemberType.Unknown
        };
    }
}