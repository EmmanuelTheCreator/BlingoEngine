namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a property value for the movie.
/// </summary>
/// <param name="Prop">Property name.</param>
/// <param name="Value">Property value.</param>
public sealed record MoviePropertyDto(string Prop, string Value);
