namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a property value for a cast member.
/// </summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="NumberInCast">Member number within its cast library.</param>
/// <param name="MemberName">Name of the cast member.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">Property value.</param>
public sealed record MemberPropertyDto(int CastLibNum, int NumberInCast, string MemberName, string Prop, string Value);
