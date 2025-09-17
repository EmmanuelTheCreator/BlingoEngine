using BlingoEngine.Movies;

namespace BlingoEngine.Transitions;

/// <summary>
/// Abstraction for playing transition animations between movie frames.
/// </summary>
public interface IBlingoTransitionPlayer : IDisposable
{
    /// <summary>Starts a transition based on the provided sprite.</summary>
    /// <returns><c>true</c> if the transition was successfully started; otherwise, <c>false</c>.</returns>
    bool Start(BlingoTransitionSprite sprite);

    /// <summary>Advances the transition by one tick.</summary>
    void Tick();

    /// <summary>Indicates whether the player is currently capturing or playing.</summary>
    bool IsActive { get; }
}


