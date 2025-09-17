namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a style range applied to a text member.
/// </summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="Start">Start index of the styled range.</param>
/// <param name="End">End index of the styled range.</param>
/// <param name="Style">Style property name.</param>
/// <param name="Value">Style value.</param>
public sealed record TextStyleDto(int CastLibNum, int MemberNum, int Start, int End, string Style, string Value)
    : RNetMemberRefDto(CastLibNum, MemberNum);


