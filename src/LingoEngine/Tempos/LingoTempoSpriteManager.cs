using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.Tempos;


public interface ILingoTempoSpriteManager : ILingoSpriteManager<LingoTempoSprite>
{
    public int Tempo { get; }
    LingoTempoSprite Add(int frameNumber, Action<LingoTempoSprite>? configure = null);
    LingoTempoSprite Add(int frameNumber, LingoTempoSpriteSettings settings);
    void ChangeTempo(LingoTempoSprite lingoTempoSprite);
   
}
internal class LingoTempoSpriteManager : LingoSpriteManager<LingoTempoSprite>, ILingoTempoSpriteManager
{
    private int _tempo = 30;  // Default frame rate (FPS)

    public int Tempo
    {
        get => _tempo;
        private set
        {
            if (value > 0)
                _tempo = value;
        }
    }
    public LingoTempoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoTempoSprite.SpriteNumOffset, movie, environment)
    {
    }

    protected override LingoTempoSprite OnCreateSprite(LingoMovie movie, Action<LingoTempoSprite> onRemove) => new LingoTempoSprite(_environment, onRemove);

    public LingoTempoSprite Add(int frameNumber, Action<LingoTempoSprite>? configure = null)
    {
        var sprite = AddSprite(1, "TempoChange_" + frameNumber);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        if (configure != null)
            configure(sprite);
        
        return sprite;
    }
    public LingoTempoSprite Add(int frameNumber, LingoTempoSpriteSettings settings)
    {
        var sprite = Add(frameNumber);
        sprite.SetSettings(settings);
        return sprite;
    }
    protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member)
    {
        var sprite = Add(begin);
     
        return sprite;
    }
    public void ChangeTempo(int value)
    {
        if (value > 0 && value < 60)
            _tempo = value;
        _environment.Clock.FrameRate = _tempo;
    }
    public void ChangeTempo(LingoTempoSprite lingoTempoSprite) => ChangeTempo(lingoTempoSprite.Tempo);

    
}
