using LingoEngine.Members;
using AbstUI.Primitives;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Animations;

namespace LingoEngine.Stages;

/// <summary>
/// You have one stage for all movies
/// </summary>
public class LingoStage : ILingoStage
{
    private readonly ILingoFrameworkStage _lingoFrameworkMovieStage;

    public int Width { get; set; } = 640;
    public int Height { get; set; } = 480;
    public AColor BackgroundColor { get; set; }

    public LingoMovie? ActiveMovie { get; private set; }
    public LingoMember? MouseMemberUnderMouse
    {
        get
        {
            if (ActiveMovie == null) return null;
            return ActiveMovie.MouseMemberUnderMouse();
        }
    }

    public ILingoFrameworkStage FrameworkObj() => _lingoFrameworkMovieStage;
    public T Framework<T>() where T : class, ILingoFrameworkStage => (T)_lingoFrameworkMovieStage;
    public LingoStage(ILingoFrameworkStage godotInstance)
    {
        _lingoFrameworkMovieStage = godotInstance;
        BackgroundColor = AColors.Black;
    }

    public void AddKeyFrame(LingoSprite2D sprite)
    {
        if (ActiveMovie == null)
            return;
        int frame = ActiveMovie.CurrentFrame;
        sprite.AddKeyframes(sprite.ToKeyFrameSetting(frame));
    }

    public void UpdateKeyFrame(LingoSprite2D sprite)
    {
        if (ActiveMovie == null)
            return;
        int frame = ActiveMovie.CurrentFrame;
        sprite.UpdateKeyframe(sprite.ToKeyFrameSetting(frame));
    }

    public void SetSpriteTweenOptions(LingoSprite2D sprite, bool positionEnabled, bool sizeEnabled, bool rotationEnabled,
        bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
        float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
    {
        sprite.SetSpriteTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
            foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
            curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
    }

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        if (ActiveMovie != null)
            ActiveMovie.Hide();
        ActiveMovie = lingoMovie;
        if (lingoMovie != null)
            lingoMovie.Show();
        _lingoFrameworkMovieStage.SetActiveMovie(lingoMovie);
    }

    public LingoSprite2D? GetSpriteUnderMouse()
    {
        if (ActiveMovie == null)
            return null;

        bool skipLockedSprites = !ActiveMovie.IsPlaying;
        return ActiveMovie.GetSpriteUnderMouse(skipLockedSprites);
    }

    public Animations.LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite2D sprite)
    {
        if (sprite == null) return null;
        return sprite.CallActor<Animations.LingoSpriteAnimator, Animations.LingoSpriteMotionPath>(
            a => a.GetMotionPath(sprite.BeginFrame, sprite.EndFrame));
    }

    public IAbstTexture2D GetScreenshot()
        => _lingoFrameworkMovieStage.GetScreenshot();

    public void ShowTransition(IAbstTexture2D startTexture)
        => _lingoFrameworkMovieStage.ShowTransition(startTexture);

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        => _lingoFrameworkMovieStage.UpdateTransitionFrame(texture, targetRect);

    public void HideTransition()
        => _lingoFrameworkMovieStage.HideTransition();
}
