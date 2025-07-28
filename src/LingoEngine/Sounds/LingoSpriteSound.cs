using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Sounds;

public class LingoSpriteSound : LingoSprite
{
    private Action<LingoSpriteSound> _onRemoveMe;
  

    public int Channel { get; protected set; }
    public LingoMemberSound Sound { get; protected set; }


#pragma warning disable CS8618 
    public LingoSpriteSound(ILingoMovieEnvironment environment, Action<LingoSpriteSound> onRemoveMe) : base(environment)
#pragma warning restore CS8618 
    {
        _onRemoveMe = onRemoveMe;
    }

    internal void Init(int channel, int beginFrame, int endFrame, LingoMemberSound sound)
    {
        Channel = channel;
        SpriteNum = channel + 3;// Tempo = 1, Colorpalette= 2, transition = 3, Sound = 4
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        Sound = sound;
        Name = sound.Name ?? string.Empty;
    }

    public override void RemoveMe()
    {
        _onRemoveMe(this);
    }
}
