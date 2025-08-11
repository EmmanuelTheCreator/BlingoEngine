using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public interface ILingoSpriteTransitionManager : ILingoSpriteManager<LingoTransitionSprite>
{
    LingoTransitionSprite Add(int frameNumber, LingoTransitionFrameSettings? settings = null);
    LingoTransitionSprite Add(int begin, LingoTransitionMember transitionMember);
}
internal class LingoSpriteTransitionManager : LingoSpriteManager<LingoTransitionSprite>, ILingoSpriteTransitionManager
{
 
    public LingoSpriteTransitionManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoTransitionSprite.SpriteNumOffset, movie, environment)
    {
    }

    protected override LingoTransitionSprite OnCreateSprite(LingoMovie movie, Action<LingoTransitionSprite> onRemove) 
        => new LingoTransitionSprite(_environment.Events,_environment.CastLibsContainer.ActiveCast, onRemove);

    public LingoTransitionSprite Add(int frameNumber, LingoTransitionFrameSettings? settings = null)
    {
        var sprite = AddSprite(1, "TransitionChange_" + frameNumber);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        if (settings != null)
            sprite.SetSettings(settings);
        
        return sprite;
    }

    public LingoTransitionSprite Add(int begin, LingoTransitionMember transitionMember)
    {
        var sprite = AddSprite(begin);
        sprite.SetMember(transitionMember);
        return sprite;
    }

    protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member)
    {
        var sprite = Add(begin);
        if (member is LingoTransitionMember memberTyped)
            sprite.SetMember(memberTyped);
        return sprite;
    }
}
