using AbstUI.Components;
using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Stages;

/// <summary>
/// Lingo Stage interface.
/// </summary>
public interface IBlingoStage : IAbstLayoutNode
{
    BlingoMovie? ActiveMovie { get; }
    AColor BackgroundColor { get; set; }
    bool IsDirty { get; }
    BlingoMember? MouseMemberUnderMouse { get; }

    void AddKeyFrame(BlingoSprite2D sprite);

    //T Framework<T>() where T : IBlingoFrameworkStage, IAbstFrameworkNode;
    //IBlingoFrameworkStage FrameworkObj();
    BlingoSpriteMotionPath? GetSpriteMotionPath(BlingoSprite2D sprite);
    BlingoSprite2D? GetSpriteUnderMouse();
    void SetActiveMovie(BlingoMovie? blingoMovie);
    void SetSpriteTweenOptions(BlingoSprite2D sprite, bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
    void UpdateKeyFrame(BlingoSprite2D sprite);


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

