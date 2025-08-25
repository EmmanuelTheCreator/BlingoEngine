namespace LingoEngine.Director.Contracts;

/// <summary>
/// Identification payload exchanged when establishing a session.
/// </summary>
public sealed record HelloDto(string ProjectId, string ClientId, string Version);
