namespace LingoEngine.Director.Contracts;

/// <summary>
/// Describes a film loop instance within the movie.
/// </summary>
/// <param name="Name">Name of the film loop.</param>
/// <param name="StartFrame">First frame of the loop.</param>
/// <param name="EndFrame">Last frame of the loop.</param>
/// <param name="IsPlaying">Whether the film loop is currently playing.</param>
public sealed record FilmLoopDto(string Name, int StartFrame, int EndFrame, bool IsPlaying);
