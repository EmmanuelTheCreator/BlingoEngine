using LingoEngine.Sprites;
using LingoEngine.Members;
using LingoEngine.Events;

namespace LingoEngine.Sounds;

public class LingoSpriteSound : LingoSprite, ILingoSpriteWithMember
{
    public const int SpriteNumOffset = 3;
    private readonly ILingoSound _sound;
    private Action<LingoSpriteSound> _onRemoveMe;
    private LingoSoundChannel? _soundChannel;

    public int Channel { get; protected set; }
    public LingoMemberSound? Sound { get; protected set; }
    public override int SpriteNumWithChannel => SpriteNumOffset + Channel;


#pragma warning disable CS8618 
    public LingoSpriteSound(ILingoSound sound, ILingoEventMediator mediator, Action<LingoSpriteSound> onRemoveMe) : base(mediator)
#pragma warning restore CS8618 
    {
        _sound = sound;
        _onRemoveMe = onRemoveMe;
    }

    internal void Init(int channel, int beginFrame, int endFrame, LingoMemberSound sound)
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

    private void SetSound(LingoMemberSound? sound)
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

    public override Action<LingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<LingoSprite> action = s => { };
        var member = Sound;
        action = s =>
        {
            baseAction(s);
            var sprite = (LingoSpriteSound)s;
            sprite.SetSound(member);
        };

        return action;
    }

    protected override LingoSpriteState CreateState() => new LingoSpriteSoundState();

    protected override void OnLoadState(LingoSpriteState state)
    {
        if (state is not LingoSpriteSoundState s) return;
        SetSound(s.Sound);
    }

    protected override void OnGetState(LingoSpriteState state)
    {
        if (state is not LingoSpriteSoundState s) return;
        s.Sound = Sound;
    }

    public ILingoMember? GetMember() => Sound;

    public void MemberHasBeenRemoved()
    {
        Sound = null;
    }
}
