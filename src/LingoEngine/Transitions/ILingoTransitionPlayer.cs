using LingoEngine.Movies;

namespace LingoEngine.Transitions;

/// <summary>
/// Abstraction for playing transition animations between movie frames.
/// </summary>
public interface ILingoTransitionPlayer
{
    /// <summary>Starts a transition based on the provided sprite.</summary>
    /// <returns><c>true</c> if the transition was successfully started; otherwise, <c>false</c>.</returns>
    bool Start(LingoTransitionSprite sprite);

    /// <summary>Captures the destination frame once it has been rendered.</summary>
    void CaptureToFrame();

    /// <summary>Advances the transition by one tick.</summary>
    void Tick();

    /// <summary>Indicates whether the player is currently capturing or playing.</summary>
    bool IsActive { get; }
}

/// <summary>
/// Fallback implementation used on frameworks without transition support.
/// </summary>
internal sealed class NullTransitionPlayer : ILingoTransitionPlayer
{
    public bool IsActive => false;
    public bool Start(LingoTransitionSprite sprite) => false;
    public void CaptureToFrame() { }
    public void Tick() { }
}
