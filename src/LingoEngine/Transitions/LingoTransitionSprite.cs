using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public class LingoTransitionSprite : LingoSprite
{
    private readonly Action<LingoTransitionSprite> _removeMe;

    public int Frame { get; set; }
    public int TransitionId { get; set; }
    public LingoTransitionSprite(ILingoMovieEnvironment environment, Action<LingoTransitionSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void RemoveMe()
    {
        _removeMe(this);
    }
}
