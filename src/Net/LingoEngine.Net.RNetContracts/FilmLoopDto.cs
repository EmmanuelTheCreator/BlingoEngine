namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Describes a film loop instance within the movie.
/// </summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="StartFrame">First frame of the loop.</param>
/// <param name="EndFrame">Last frame of the loop.</param>
/// <param name="IsPlaying">Whether the film loop is currently playing.</param>
public sealed record FilmLoopDto(int CastLibNum, int MemberNum, int StartFrame, int EndFrame, bool IsPlaying)
    : RNetMemberRefDto(CastLibNum, MemberNum);

