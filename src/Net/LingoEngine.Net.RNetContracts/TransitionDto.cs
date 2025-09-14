namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a transition applied between frames.
/// </summary>
/// <param name="Frame">Frame number where the transition occurs.</param>
/// <param name="Type">Transition type identifier.</param>
/// <param name="Duration">Duration of the transition in frames.</param>
public sealed record TransitionDto(int Frame, string Type, int Duration);
