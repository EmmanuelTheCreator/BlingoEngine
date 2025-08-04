using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public interface ILingoSpriteTransitionManager : ILingoSpriteManager<LingoTransitionSprite>
{
    LingoTransitionSprite Add(int frameNumber, int valueTodoAndAddMoreValues);
}
internal class LingoSpriteTransitionManager : LingoSpriteManager<LingoTransitionSprite>, ILingoSpriteTransitionManager
{
 
    public LingoSpriteTransitionManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoTransitionSprite.SpriteNumOffset, movie, environment)
    {
    }

    protected override LingoTransitionSprite OnCreateSprite(LingoMovie movie, Action<LingoTransitionSprite> onRemove) => new LingoTransitionSprite(_environment, onRemove);

    public LingoTransitionSprite Add(int frameNumber, int valueTodoAndAddMoreValues)
    {
        var sprite = AddSprite(1, "TransitionChange_" + frameNumber, c => c.TransitionId = valueTodoAndAddMoreValues);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        return sprite;
    }
}
