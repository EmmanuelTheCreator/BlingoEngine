using BlingoEngine.Sprites;
using BlingoEngine.Members;
using BlingoEngine.Events;

namespace BlingoEngine.Sounds;

public class BlingoSpriteSound : BlingoSprite, IBlingoSpriteWithMember
{
    public const int SpriteNumOffset = 3;
    private readonly IBlingoSound _sound;
    private Action<BlingoSpriteSound> _onRemoveMe;
    private BlingoSoundChannel? _soundChannel;

    public int Channel { get; protected set; }
    public BlingoMemberSound? Sound { get; protected set; }
    public override int SpriteNumWithChannel => SpriteNumOffset + Channel;


#pragma warning disable CS8618 
    public BlingoSpriteSound(IBlingoSound sound, IBlingoEventMediator mediator, Action<BlingoSpriteSound> onRemoveMe) : base(mediator)
#pragma warning restore CS8618 
    {
        _sound = sound;
        _onRemoveMe = onRemoveMe;
    }

    internal void Init(int channel, int beginFrame, int endFrame, BlingoMemberSound sound)
    {
        // Tempo = 1, Colorpalette= 2, transition = 3, Sound = 4/5 , 6 Framescripts
        Channel = channel;
        SpriteNum = channel;
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        SetSound(sound);
        Name = sound.Name ?? string.Empty;
        _soundChannel = _sound.Channel(channel);
    }

    private void SetSound(BlingoMemberSound? sound)
    {
        if (Sound == sound || sound == null) return;
        Sound = sound;
        Sound.UsedBy(this);
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        if (Sound != null && _soundChannel != null)
            _soundChannel.Play(Sound);
    }
    protected override void EndSprite()
    {
        _soundChannel?.Stop();
        base.EndSprite();
    }

    public override void OnRemoveMe()
    {
        Sound?.ReleaseFromRefUser(this);
        _onRemoveMe(this);
    }

    public override Action<BlingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<BlingoSprite> action = s => { };
        var member = Sound;
        action = s =>
        {
            baseAction(s);
            var sprite = (BlingoSpriteSound)s;
            sprite.SetSound(member);
        };

        return action;
    }

    protected override BlingoSpriteState CreateState() => new BlingoSpriteSoundState();

    protected override void OnLoadState(BlingoSpriteState state)
    {
        if (state is not BlingoSpriteSoundState s) return;
        SetSound(s.Sound);
    }

    protected override void OnGetState(BlingoSpriteState state)
    {
        if (state is not BlingoSpriteSoundState s) return;
        s.Sound = Sound;
    }

    public IBlingoMember? GetMember() => Sound;

    public void MemberHasBeenRemoved()
    {
        Sound = null;
    }
}

