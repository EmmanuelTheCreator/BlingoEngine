using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Tempos;

public class LingoTempoSprite : LingoSprite
{
    private readonly Action<LingoTempoSprite> _removeMe;

    public int Frame { get; set; }
    public int Tempo { get; set; } = 30;
    public LingoTempoSprite(ILingoMovieEnvironment environment, Action<LingoTempoSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
    }

    protected override void BeginSprite()
    {
        base.BeginSprite();
        _environment.Movie.Tempos.ChangeTempo(this);
    }

    public override void RemoveMe()
    {
        _removeMe(this);
    }
}
