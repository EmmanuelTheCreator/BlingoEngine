namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents the state of a sound at a given frame.
/// </summary>
/// <param name="Frame">Frame number the sound information applies to.</param>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="IsPlaying">Whether the sound is currently playing.</param>
public sealed record SoundEventDto(int Frame, int CastLibNum, int MemberNum, bool IsPlaying)
    : RNetMemberRefDto(CastLibNum, MemberNum);

