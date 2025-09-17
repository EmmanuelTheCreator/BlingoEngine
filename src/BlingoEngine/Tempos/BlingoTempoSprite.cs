using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Events;
using System;


namespace BlingoEngine.Tempos;

public enum BlingoTempoSpriteAction
{
    ChangeTempo,
    WaitSeconds,
    WaitForUserInput,
    WaitForCuePoint
}
public class BlingoTempoSprite : BlingoSprite
{
    public const int SpriteNumOffset = 0;
    private readonly IBlingoMovieEnvironment _environment;
    private readonly Action<BlingoTempoSprite> _removeMe;
    private int _tempo = 30;

    public int Frame { get; set; }

    /// <summary>
    /// Determines which action this tempo sprite performs.
    /// Only one action can be active at a time.
    /// </summary>
    public BlingoTempoSpriteAction Action { get; set; }
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
    /// Number of seconds to wait when <see cref="Action"/> is <see cref="BlingoTempoSpriteAction.WaitSeconds"/>.
    /// </summary>
    public float WaitSeconds { get; set; }

    /// <summary>
    /// Cue channel to wait on when <see cref="Action"/> is <see cref="BlingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CueChannel { get; set; }

    /// <summary>
    /// Cue point to wait for when <see cref="Action"/> is <see cref="BlingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CuePoint { get; set; }

        private class WaitForInputSubscription : IHasMouseDownEvent, IHasKeyDownEvent
        {
            private readonly BlingoTempoSprite _owner;
            public WaitForInputSubscription(BlingoTempoSprite owner) => _owner = owner;
            public void MouseDown(BlingoMouseEvent mouse) => _owner.Resume();
            public void KeyDown(BlingoKeyEvent key) => _owner.Resume();
        }

    private WaitForInputSubscription? _waitForInputSubscription;
    public BlingoTempoSprite(IBlingoMovieEnvironment environment, Action<BlingoTempoSprite> removeMe) : base(environment.Events)
    {
        _environment = environment;
        _removeMe = removeMe;
        IsSingleFrame = true;
        Action = BlingoTempoSpriteAction.ChangeTempo;
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        switch (Action)
        {
            case BlingoTempoSpriteAction.ChangeTempo:
                _environment.Movie.Tempos.ChangeTempo(this);
                break;
            case BlingoTempoSpriteAction.WaitSeconds:
                var ticks = (int)(_environment.Clock.FrameRate * WaitSeconds);
                _environment.Movie.Delay(ticks);
                break;
            case BlingoTempoSpriteAction.WaitForUserInput:
                _environment.Movie.WaitForInput();
                _waitForInputSubscription = new WaitForInputSubscription(this);
                _eventMediator.Subscribe(_waitForInputSubscription, SpriteNum);
                break;
            case BlingoTempoSpriteAction.WaitForCuePoint:
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
    public void SetSettings(BlingoTempoSpriteSettings settings)
    {
        _tempo = settings.Tempo;
        Action = settings.Action;
        WaitSeconds = settings.WaitSeconds;
        CueChannel = settings.CueChannel;
        CuePoint = settings.CuePoint;
        UpdateName(settings);
    }

    private void UpdateName(BlingoTempoSpriteSettings settings)
    {
        switch (Action)
        {
            case BlingoTempoSpriteAction.ChangeTempo: Name = "Tempo to " + settings.Tempo + "fps."; break;
            case BlingoTempoSpriteAction.WaitSeconds: Name = "Wait for " + settings.WaitSeconds + "s."; break;
            case BlingoTempoSpriteAction.WaitForUserInput: Name = "Wait for user input"; break;
            case BlingoTempoSpriteAction.WaitForCuePoint: Name = "Wait for CuePoint " + settings.CueChannel + "/" + settings.CuePoint; break;
            default:
                break;
        }
    }

    public BlingoTempoSpriteSettings? GetSettings()
    {
        var settings = new BlingoTempoSpriteSettings
        {
            Tempo = _tempo,
            Action = Action,
            WaitSeconds = WaitSeconds,
            CueChannel = CueChannel,
            CuePoint = CuePoint
        }; 
        return settings;
    }

    public override Action<BlingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<BlingoSprite> action = s => { };
        var settings = GetSettings();
        
        action = s =>
        {
            baseAction(s);
            var sprite = (BlingoTempoSprite)s;
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }
}

