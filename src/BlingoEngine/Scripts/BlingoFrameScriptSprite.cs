using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using System.Reflection;
using BlingoEngine.Members;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Core;
using Microsoft.Extensions.DependencyInjection;


namespace BlingoEngine.Scripts;

public class BlingoFrameScriptSprite : BlingoSprite, IBlingoSpriteWithMember
{
    public const int SpriteNumOffset = 5;
    private readonly IBlingoPlayer _player;
    private readonly IBlingoFrameworkFactory _frameworkFactory;
    private Action<BlingoFrameScriptSprite> _onRemoveMe;
  

    public int Channel { get; protected set; }
    public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;
    public BlingoMemberScript Member { get; protected set; }
    public BlingoSpriteBehavior? Behavior { get; private set; }


#pragma warning disable CS8618
    public BlingoFrameScriptSprite(IBlingoPlayer player,IBlingoFrameworkFactory frameworkFactory, IBlingoEventMediator eventMediator, Action<BlingoFrameScriptSprite> onRemoveMe) : base(eventMediator)
#pragma warning restore CS8618 
    {
        _player = player;
        _frameworkFactory = frameworkFactory;
        _onRemoveMe = onRemoveMe;
        IsSingleFrame = true;
    }

    internal void Init(int channel, int beginFrame, int endFrame, BlingoMemberScript frameScript)
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
    internal T SetBehavior<T>() where T : BlingoSpriteBehavior
    {
        if (_player.ActiveMovie == null) throw new Exception("No active movie found to set behavior on.");
        var behavior = ((BlingoMovie)_player.ActiveMovie).GetServiceProvider().GetRequiredService<T>();
        Behavior = behavior;
        
        //Behavior.SetMe(this);

        return behavior;
    } 
   

    public override Action<BlingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<BlingoSprite> action = s => { };
        var behaviorType = Behavior != null ? Behavior.GetType(): null;
        var userProperties = Behavior != null ? Behavior.UserProperties.Clone() : null;
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (BlingoFrameScriptSprite)s;
            sprite.Member = member;
            member.UsedBy(sprite);
            if (behaviorType != null)
            {
                var method = typeof(BlingoFrameScriptSprite).GetMethod(nameof(SetBehavior), BindingFlags.Instance | BindingFlags.NonPublic);
                var generic = method!.MakeGenericMethod(behaviorType);
                var behaviorNew = (BlingoSpriteBehavior)generic.Invoke(sprite, null)!;
                if (userProperties != null)
                    behaviorNew.SetUserProperties(userProperties);

            }
        };

        return action;
    }

    public void SetMember(BlingoMemberScript blingoMemberScript)
    {
        Member?.ReleaseFromRefUser(this);
        Member = blingoMemberScript;
        Member.UsedBy(this);
    }

    public IBlingoMember? GetMember() => Member;

    public void MemberHasBeenRemoved()
    {
        Member = null!;
    }
}

