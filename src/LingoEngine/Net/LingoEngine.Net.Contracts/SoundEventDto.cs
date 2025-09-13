namespace LingoEngine.Net.Contracts;

/// <summary>
/// Represents the state of a sound at a given frame.
/// </summary>
/// <param name="Frame">Frame number the sound information applies to.</param>
/// <param name="SoundName">Identifier of the sound.</param>
/// <param name="IsPlaying">Whether the sound is currently playing.</param>
public sealed record SoundEventDto(int Frame, string SoundName, bool IsPlaying);
