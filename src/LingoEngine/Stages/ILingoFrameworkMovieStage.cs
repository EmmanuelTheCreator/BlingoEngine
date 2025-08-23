using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Stages;

/// <summary>
/// Represents the top-level window or stage. Implementations update the
/// display when the active <see cref="LingoMovie"/> changes.
/// </summary>
public interface ILingoFrameworkStage
{
    LingoStage LingoStage { get; }
    /// <summary>Sets the currently active movie.</summary>
    void SetActiveMovie(LingoMovie? lingoMovie);
    void ApplyPropertyChanges();

    float Scale { get; set; }

    /// <summary>Captures a screenshot of the current stage.</summary>
    IAbstTexture2D GetScreenshot();

    /// <summary>Shows the transition overlay above the sprite layer.</summary>
    void ShowTransition(IAbstTexture2D startTexture);

    /// <summary>Updates the displayed transition frame.</summary>
    void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect);

    /// <summary>Hides the transition overlay and returns rendering to sprites.</summary>
    void HideTransition();
}
