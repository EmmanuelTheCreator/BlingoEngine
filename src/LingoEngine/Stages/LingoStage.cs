using LingoEngine.Members;
using AbstUI.Primitives;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Animations;
using AbstUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AbstUI.Components;

namespace LingoEngine.Stages;

/// <summary>
/// You have one stage for all movies
/// </summary>
public class LingoStage : ILingoStage, IHasPropertyChanged
{
    private readonly ILingoFrameworkStage _lingoFrameworkMovieStage;
    private AColor _backgroundColor;
    public bool IsDirty { get; private set; }
    private float _width = 640;
    public float Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            _lingoFrameworkMovieStage.Width = value;
            IsDirty = true;
            OnPropertyChanged();
        }
    }
    private float _height = 480;
    public float Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            _lingoFrameworkMovieStage.Height = value;
            IsDirty = true;
            OnPropertyChanged();
        }
    }
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor.Equals(value)) return;
            _backgroundColor = value;
            IsDirty = true;
        }
    }

    public LingoMovie? ActiveMovie { get; private set; }
    public LingoMember? MouseMemberUnderMouse
    {
        get
        {
            if (ActiveMovie == null) return null;
            return ActiveMovie.MouseMemberUnderMouse();
        }
    }

    public string Name { get; set; } = "TheStage";
    public bool Visibility { get => _lingoFrameworkMovieStage.Visibility; set => _lingoFrameworkMovieStage.Visibility = value; }
    public AMargin Margin { get => _lingoFrameworkMovieStage.Margin; set => _lingoFrameworkMovieStage.Margin = value; }
    public int ZIndex { get => _lingoFrameworkMovieStage.ZIndex; set => _lingoFrameworkMovieStage.ZIndex = value; }
    IAbstFrameworkNode IAbstNode.FrameworkObj { get => _lingoFrameworkMovieStage; set => throw new NotImplementedException(); } // not allowed to set.

    public T Framework<T>() where T: IAbstFrameworkNode => (T)_lingoFrameworkMovieStage;



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

    public LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite2D sprite)
    {
        if (sprite == null) return null;
        return sprite.CallActor<LingoSpriteAnimator, LingoSpriteMotionPath>(
            a => a.GetMotionPath(sprite.BeginFrame, sprite.EndFrame));
    }
    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
        => _lingoFrameworkMovieStage.RequestNextFrameScreenshot(onCaptured);
    public IAbstTexture2D GetScreenshot()
        => _lingoFrameworkMovieStage.GetScreenshot();

    public void ShowTransition(IAbstTexture2D startTexture)
        => _lingoFrameworkMovieStage.ShowTransition(startTexture);

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        => _lingoFrameworkMovieStage.UpdateTransitionFrame(texture, targetRect);

    public void HideTransition()
        => _lingoFrameworkMovieStage.HideTransition();

    public void StageChangedApplied()
    {
        IsDirty = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

   
}
