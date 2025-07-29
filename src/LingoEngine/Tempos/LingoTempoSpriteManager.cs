using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Tempos;


public interface ILingoTempoSpriteManager : ILingoSpriteManager<LingoTempoSprite>
{
    public int Tempo { get; }
    LingoTempoSprite Add(int frameNumber, int value);
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
    public LingoTempoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
    {
    }

    protected override LingoTempoSprite OnCreateSprite(LingoMovie movie, Action<LingoTempoSprite> onRemove) => new LingoTempoSprite(_environment, onRemove);

    public LingoTempoSprite Add(int frameNumber, int value)
    {
        var sprite = AddSprite(1, "TempoChange_" + frameNumber,c => c.Tempo = value);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
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
