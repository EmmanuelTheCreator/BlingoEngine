using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public interface ILingoSpriteTransitionManager : ILingoSpriteManager<LingoTransitionSprite>
{

}
internal class LingoSpriteTransitionManager : LingoSpriteManager<LingoTransitionSprite>, ILingoSpriteTransitionManager
{
 
    public LingoSpriteTransitionManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
    {
    }

    protected override LingoTransitionSprite OnCreateSprite(LingoMovie movie, Action<LingoTransitionSprite> onRemove) => new LingoTransitionSprite(_environment, onRemove);
}
