namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents the play state of the movie.
/// </summary>
/// <param name="Frame">Current frame number.</param>
/// <param name="Tempo">Current playback tempo.</param>
/// <param name="IsPlaying">Whether playback is active.</param>
public sealed record MovieStateDto(int Frame, int Tempo, bool IsPlaying);
public sealed record MovieJsonDto(string json);
