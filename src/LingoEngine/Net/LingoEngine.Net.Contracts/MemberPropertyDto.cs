namespace LingoEngine.Net.Contracts;

/// <summary>
/// Represents a property value for a cast member.
/// </summary>
/// <param name="MemberName">Name of the cast member.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">Property value.</param>
public sealed record MemberPropertyDto(string MemberName, string Prop, string Value);
