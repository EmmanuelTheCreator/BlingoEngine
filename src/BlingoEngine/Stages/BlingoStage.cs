using BlingoEngine.Members;
using AbstUI.Primitives;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Animations;
using AbstUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AbstUI.Components;

namespace BlingoEngine.Stages;

/// <summary>
/// You have one stage for all movies
/// </summary>
public class BlingoStage : IBlingoStage, IHasPropertyChanged
{
    private readonly IBlingoFrameworkStage _blingoFrameworkMovieStage;
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
            _blingoFrameworkMovieStage.Width = value;
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
            _blingoFrameworkMovieStage.Height = value;
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

    public BlingoMovie? ActiveMovie { get; private set; }
    public BlingoMember? MouseMemberUnderMouse
    {
        get
        {
            if (ActiveMovie == null) return null;
            return ActiveMovie.MouseMemberUnderMouse();
        }
    }

    public string Name { get; set; } = "TheStage";
    public bool Visibility { get => _blingoFrameworkMovieStage.Visibility; set => _blingoFrameworkMovieStage.Visibility = value; }
    public AMargin Margin { get => _blingoFrameworkMovieStage.Margin; set => _blingoFrameworkMovieStage.Margin = value; }
    public int ZIndex { get => _blingoFrameworkMovieStage.ZIndex; set => _blingoFrameworkMovieStage.ZIndex = value; }
    IAbstFrameworkNode IAbstNode.FrameworkObj { get => _blingoFrameworkMovieStage; set => throw new NotImplementedException(); } // not allowed to set.

    public T Framework<T>() where T: IAbstFrameworkNode => (T)_blingoFrameworkMovieStage;



    public BlingoStage(IBlingoFrameworkStage godotInstance)
    {
        _blingoFrameworkMovieStage = godotInstance;
        BackgroundColor = AColors.Black;
    }

    public void AddKeyFrame(BlingoSprite2D sprite)
    {
        if (ActiveMovie == null)
            return;
        int frame = ActiveMovie.CurrentFrame;
        sprite.AddKeyframes(sprite.ToKeyFrameSetting(frame));
    }

    public void UpdateKeyFrame(BlingoSprite2D sprite)
    {
        if (ActiveMovie == null)
            return;
        int frame = ActiveMovie.CurrentFrame;
        sprite.UpdateKeyframe(sprite.ToKeyFrameSetting(frame));
    }

    public void SetSpriteTweenOptions(BlingoSprite2D sprite, bool positionEnabled, bool sizeEnabled, bool rotationEnabled,
        bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
        float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
    {
        sprite.SetSpriteTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
            foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
            curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
    }

    public void SetActiveMovie(BlingoMovie? blingoMovie)
    {
        if (ActiveMovie != null)
            ActiveMovie.Hide();
        ActiveMovie = blingoMovie;
        if (blingoMovie != null)
            blingoMovie.Show();
        _blingoFrameworkMovieStage.SetActiveMovie(blingoMovie);
    }

    public BlingoSprite2D? GetSpriteUnderMouse()
    {
        if (ActiveMovie == null)
            return null;

        bool skipLockedSprites = !ActiveMovie.IsPlaying;
        return ActiveMovie.GetSpriteUnderMouse(skipLockedSprites);
    }

    public BlingoSpriteMotionPath? GetSpriteMotionPath(BlingoSprite2D sprite)
    {
        if (sprite == null) return null;
        return sprite.CallActor<BlingoSpriteAnimator, BlingoSpriteMotionPath>(
            a => a.GetMotionPath(sprite.BeginFrame, sprite.EndFrame));
    }
    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
        => _blingoFrameworkMovieStage.RequestNextFrameScreenshot(onCaptured);
    public IAbstTexture2D GetScreenshot()
        => _blingoFrameworkMovieStage.GetScreenshot();

    public void ShowTransition(IAbstTexture2D startTexture)
        => _blingoFrameworkMovieStage.ShowTransition(startTexture);

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        => _blingoFrameworkMovieStage.UpdateTransitionFrame(texture, targetRect);

    public void HideTransition()
        => _blingoFrameworkMovieStage.HideTransition();

    public void StageChangedApplied()
    {
        IsDirty = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

   
}

