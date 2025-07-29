using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;


namespace LingoEngine.Scripts;

public class LingoSpriteFrameScript : LingoSprite
{
    public const int FrameScriptSpriteNum = 6;
    private Action<LingoSpriteFrameScript> _onRemoveMe;
  

    public int Channel { get; protected set; }
    public LingoMemberScript FrameScript { get; protected set; }
    public LingoSpriteBehavior? Behavior { get; private set; }


#pragma warning disable CS8618
    public LingoSpriteFrameScript(ILingoMovieEnvironment environment, Action<LingoSpriteFrameScript> onRemoveMe) : base(environment)
#pragma warning restore CS8618 
    {
        _onRemoveMe = onRemoveMe;
        IsSingleFrame = true;
    }

    internal void Init(int channel, int beginFrame, int endFrame, LingoMemberScript frameScript)
    {
        Channel = channel;
        SpriteNum = FrameScriptSpriteNum;// Tempo = 1, Colorpalette= 2, transition = 3, Audio1 = 4, Audio2 = 5, FrameScript = 6
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        FrameScript = frameScript;
        Name = frameScript.Name ?? string.Empty;
    }

    public override void RemoveMe()
    {
        if (Behavior != null)
            _environment.Events.Unsubscribe(Behavior);
        _onRemoveMe(this);
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        if (Behavior == null) return;
        _environment.Events.Subscribe(Behavior, FrameScriptSpriteNum);
        if (Behavior is IHasBeginSpriteEvent beginSpriteEvent)
            beginSpriteEvent.BeginSprite();
    }
    protected override void EndSprite()
    {
        if (Behavior is IHasEndSpriteEvent endSpriteEvent)
            endSpriteEvent.EndSprite();
        base.EndSprite();
        if (Behavior == null) return;
        _environment.Events.Unsubscribe(Behavior);
    }
    internal T SetBehavior<T>() where T : LingoSpriteBehavior
    {
        var behavior = _environment.Factory.CreateBehavior<T>((LingoMovie)_environment.Movie);
        Behavior = behavior;
        
        //Behavior.SetMe(this);

        return behavior;
    }
}
