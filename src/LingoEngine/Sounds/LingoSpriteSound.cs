using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Sounds;

public class LingoSpriteSound : LingoSprite
{
    public const int SpriteNumOffset = 3;
    private Action<LingoSpriteSound> _onRemoveMe;
    private LingoSoundChannel? _soundChannel;

    public int Channel { get; protected set; }
    public LingoMemberSound? Sound { get; protected set; }
    public override int SpriteNumWithChannel => SpriteNumOffset + Channel;


#pragma warning disable CS8618 
    public LingoSpriteSound(ILingoMovieEnvironment environment, Action<LingoSpriteSound> onRemoveMe) : base(environment)
#pragma warning restore CS8618 
    {
        _onRemoveMe = onRemoveMe;
    }

    internal void Init(int channel, int beginFrame, int endFrame, LingoMemberSound sound)
    {
        // Tempo = 1, Colorpalette= 2, transition = 3, Sound = 4/5 , 6 Framescripts
        Channel = channel;
        SpriteNum = channel;
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        Sound = sound;
        Name = sound.Name ?? string.Empty;
        _soundChannel = _environment.Sound.Channel(channel);
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
        _onRemoveMe(this);
    }
}
