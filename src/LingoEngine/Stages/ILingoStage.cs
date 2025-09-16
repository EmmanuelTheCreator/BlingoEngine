using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Stages;

/// <summary>
/// Lingo Stage interface.
/// </summary>
public interface ILingoStage : IAbstLayoutNode
{
    LingoMovie? ActiveMovie { get; }
    AColor BackgroundColor { get; set; }
    bool IsDirty { get; }
    LingoMember? MouseMemberUnderMouse { get; }

    void AddKeyFrame(LingoSprite2D sprite);

    //T Framework<T>() where T : ILingoFrameworkStage, IAbstFrameworkNode;
    //ILingoFrameworkStage FrameworkObj();
    LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite2D sprite);
    LingoSprite2D? GetSpriteUnderMouse();
    void SetActiveMovie(LingoMovie? lingoMovie);
    void SetSpriteTweenOptions(LingoSprite2D sprite, bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
    void UpdateKeyFrame(LingoSprite2D sprite);


    void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured);
    /// <summary>Captures the current contents of the stage.</summary>
    IAbstTexture2D GetScreenshot();

    /// <summary>Shows the transition overlay above the sprite layer.</summary>
    void ShowTransition(IAbstTexture2D startTexture);

    /// <summary>Updates the transition frame.</summary>
    void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect);

    /// <summary>Hides the transition overlay and returns rendering to sprites.</summary>
    void HideTransition();
    /// <summary>
    /// Dirty has been applied, clear the flag.
    /// </summary>
    void StageChangedApplied();
}
