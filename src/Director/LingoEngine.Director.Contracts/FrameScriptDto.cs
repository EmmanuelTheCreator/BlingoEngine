namespace LingoEngine.Director.Contracts;

/// <summary>
/// Represents a frame script executed at a specific frame.
/// </summary>
/// <param name="Frame">Frame number the script runs on.</param>
/// <param name="Script">Script contents.</param>
public sealed record FrameScriptDto(int Frame, string Script);
