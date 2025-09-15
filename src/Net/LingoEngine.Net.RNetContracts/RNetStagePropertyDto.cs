namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a property value for the stage.
/// </summary>
/// <param name="Prop">Property name.</param>
/// <param name="Value">Property value.</param>
public sealed record RNetStagePropertyDto(string Prop, string Value);
