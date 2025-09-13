namespace LingoEngine.Net.Contracts;

/// <summary>
/// Represents a style range applied to a text member.
/// </summary>
/// <param name="MemberName">Name of the text member.</param>
/// <param name="Start">Start index of the styled range.</param>
/// <param name="End">End index of the styled range.</param>
/// <param name="Style">Style property name.</param>
/// <param name="Value">Style value.</param>
public sealed record TextStyleDto(string MemberName, int Start, int End, string Style, string Value);
