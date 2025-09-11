using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using System.Reflection;
using LingoEngine.Members;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Core;
using Microsoft.Extensions.DependencyInjection;


namespace LingoEngine.Scripts;

public class LingoFrameScriptSprite : LingoSprite, ILingoSpriteWithMember
{
    public const int SpriteNumOffset = 5;
    private readonly ILingoPlayer _player;
    private readonly ILingoFrameworkFactory _frameworkFactory;
    private Action<LingoFrameScriptSprite> _onRemoveMe;
  

    public int Channel { get; protected set; }
    public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;
    public LingoMemberScript Member { get; protected set; }
    public LingoSpriteBehavior? Behavior { get; private set; }


#pragma warning disable CS8618
    public LingoFrameScriptSprite(ILingoPlayer player,ILingoFrameworkFactory frameworkFactory, ILingoEventMediator eventMediator, Action<LingoFrameScriptSprite> onRemoveMe) : base(eventMediator)
#pragma warning restore CS8618 
    {
        _player = player;
        _frameworkFactory = frameworkFactory;
        _onRemoveMe = onRemoveMe;
        IsSingleFrame = true;
    }

    internal void Init(int channel, int beginFrame, int endFrame, LingoMemberScript frameScript)
    {
        Channel = channel;
        SpriteNum = 1;// Tempo = 1, Colorpalette= 2, transition = 3, Audio1 = 4, Audio2 = 5, FrameScript = 6
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        Member = frameScript;
        Member.UsedBy(this);
        Name = frameScript.Name ?? string.Empty;
    }

    public override void OnRemoveMe()
    {
        Member?.ReleaseFromRefUser(this);
        if (Behavior != null)
            _eventMediator.Unsubscribe(Behavior);
        _onRemoveMe(this);
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        if (Behavior == null) return;
        _eventMediator.Subscribe(Behavior, SpriteNumOffset+ SpriteNum);
        if (Behavior is IHasBeginSpriteEvent beginSpriteEvent)
            beginSpriteEvent.BeginSprite();
    }
    protected override void EndSprite()
    {
        if (Behavior is IHasEndSpriteEvent endSpriteEvent)
            endSpriteEvent.EndSprite();
        base.EndSprite();
        if (Behavior == null) return;
        _eventMediator.Unsubscribe(Behavior);
    }
    internal T SetBehavior<T>() where T : LingoSpriteBehavior
    {
        if (_player.ActiveMovie == null) throw new Exception("No active movie found to set behavior on.");
        var behavior = ((LingoPlayer)_player).ServiceProvider.GetRequiredService<T>();
        Behavior = behavior;
        
        //Behavior.SetMe(this);

        return behavior;
    } 
   

    public override Action<LingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<LingoSprite> action = s => { };
        var behaviorType = Behavior != null ? Behavior.GetType(): null;
        var userProperties = Behavior != null ? Behavior.UserProperties.Clone() : null;
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (LingoFrameScriptSprite)s;
            sprite.Member = member;
            member.UsedBy(sprite);
            if (behaviorType != null)
            {
                var method = typeof(LingoFrameScriptSprite).GetMethod(nameof(SetBehavior), BindingFlags.Instance | BindingFlags.NonPublic);
                var generic = method!.MakeGenericMethod(behaviorType);
                var behaviorNew = (LingoSpriteBehavior)generic.Invoke(sprite, null)!;
                if (userProperties != null)
                    behaviorNew.SetUserProperties(userProperties);

            }
        };

        return action;
    }

    public void SetMember(LingoMemberScript lingoMemberScript)
    {
        Member?.ReleaseFromRefUser(this);
        Member = lingoMemberScript;
        Member.UsedBy(this);
    }

    public ILingoMember? GetMember() => Member;

    public void MemberHasBeenRemoved()
    {
        Member = null!;
    }
}
