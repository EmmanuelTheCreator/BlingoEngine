namespace LingoEngine.Net.Contracts;

/// <summary>
/// Represents a tempo change at a specific frame.
/// </summary>
/// <param name="Frame">Frame number the tempo applies to.</param>
/// <param name="Tempo">Tempo value in frames per second.</param>
public sealed record TempoDto(int Frame, int Tempo);
