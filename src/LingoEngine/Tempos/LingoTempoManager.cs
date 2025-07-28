using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Tempos;


public interface ILingoSpriteTempoManager : ILingoSpriteManager<LingoTempoSprite>
{
    public int Tempo { get; }
    LingoTempoSprite Add(int frameNumber, int value);
    void ChangeTempo(LingoTempoSprite lingoTempoSprite);
   
}
internal class LingoSpriteTempoManager : LingoSpriteManager<LingoTempoSprite>, ILingoSpriteTempoManager
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
    public LingoSpriteTempoManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
    {
    }

    protected override LingoTempoSprite OnCreateSprite(LingoMovie movie, Action<LingoTempoSprite> onRemove) => new LingoTempoSprite(_environment, onRemove);

    public LingoTempoSprite Add(int frameNumber, int value)
    {
        return AddSprite(1, "TempoChange_" + frameNumber,c => c.Tempo = value);
    }

    public void ChangeTempo(int value)
    {
        if (value > 0 && value < 120)
            _tempo = value;
    }
    public void ChangeTempo(LingoTempoSprite lingoTempoSprite) => ChangeTempo(lingoTempoSprite.Tempo);
}
