namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a property value for a cast member.
/// </summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="NumberInCast">Member number within its cast library.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">Property value.</param>
public sealed record RNetMemberPropertyDto(int CastLibNum, int NumberInCast, string Prop, string Value);
