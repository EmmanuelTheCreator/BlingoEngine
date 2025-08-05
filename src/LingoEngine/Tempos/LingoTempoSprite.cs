using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Inputs.Events;
using LingoEngine.Inputs;
using LingoEngine.Events;


namespace LingoEngine.Tempos;

public enum LingoTempoSpriteAction
{
    ChangeTempo,
    WaitSeconds,
    WaitForUserInput,
    WaitForCuePoint
}
public class LingoTempoSprite : LingoSprite
{
    public const int SpriteNumOffset = 0;
    private readonly Action<LingoTempoSprite> _removeMe;
    private int _tempo = 30;

    public int Frame { get; set; }

    /// <summary>
    /// Determines which action this tempo sprite performs.
    /// Only one action can be active at a time.
    /// </summary>
    public LingoTempoSpriteAction Action { get; set; }
    public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;
    public int Tempo
    {
        get => _tempo; set
        {
            _tempo = value;
            Name = _tempo + "fps";
        }
    }

    /// <summary>
    /// Number of seconds to wait when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitSeconds"/>.
    /// </summary>
    public float WaitSeconds { get; set; }

    /// <summary>
    /// Cue channel to wait on when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CueChannel { get; set; }

    /// <summary>
    /// Cue point to wait for when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CuePoint { get; set; }

    private class WaitForInputSubscription : IHasMouseDownEvent, IHasKeyDownEvent
    {
        private readonly LingoTempoSprite _owner;
        public WaitForInputSubscription(LingoTempoSprite owner) => _owner = owner;
        public void MouseDown(LingoMouseEvent mouse) => _owner.Resume();
        public void KeyDown(ILingoKey key) => _owner.Resume();
    }

    private WaitForInputSubscription? _waitForInputSubscription;
    public LingoTempoSprite(ILingoMovieEnvironment environment, Action<LingoTempoSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
        IsSingleFrame = true;
        Action = LingoTempoSpriteAction.ChangeTempo;
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        switch (Action)
        {
            case LingoTempoSpriteAction.ChangeTempo:
                _environment.Movie.Tempos.ChangeTempo(this);
                break;
            case LingoTempoSpriteAction.WaitSeconds:
                var ticks = (int)(_environment.Clock.FrameRate * WaitSeconds);
                _environment.Movie.Delay(ticks);
                break;
            case LingoTempoSpriteAction.WaitForUserInput:
                _environment.Movie.WaitForInput();
                _waitForInputSubscription = new WaitForInputSubscription(this);
                _eventMediator.Subscribe(_waitForInputSubscription, SpriteNum);
                break;
            case LingoTempoSpriteAction.WaitForCuePoint:
                _environment.Movie.WaitForCuePoint(CueChannel, CuePoint);
                break;
        }
    }

    protected override void EndSprite()
    {
        if (_waitForInputSubscription != null)
        {
            _eventMediator.Unsubscribe(_waitForInputSubscription);
            _waitForInputSubscription = null;
        }
        base.EndSprite();
    }

    public override void OnRemoveMe()
    {
        _removeMe(this);
    }

    private void Resume() => _environment.Movie.ContinueAfterInput();
    public void SetSettings(LingoTempoSpriteSettings settings)
    {
        _tempo = settings.Tempo;
        Action = settings.Action;
        WaitSeconds = settings.WaitSeconds;
        CueChannel = settings.CueChannel;
        CuePoint = settings.CuePoint;
    }

    public LingoTempoSpriteSettings? GetSettings()
    {
        var settings = new LingoTempoSpriteSettings
        {
            Tempo = _tempo,
            Action = Action,
            WaitSeconds = WaitSeconds,
            CueChannel = CueChannel,
            CuePoint = CuePoint
        }; 
        return settings;
    }

    public override Action<LingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<LingoSprite> action = s => { };
        var settings = GetSettings();
        
        action = s =>
        {
            baseAction(s);
            var sprite = (LingoTempoSprite)s;
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }
}
