namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Identifies a cast member within a specific cast library.
/// </summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
public record RNetMemberRefDto(int CastLibNum, int MemberNum);


