using System.Linq;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Transitions;

/// <summary>
/// Lingo Sprite Transition Manager interface.
/// </summary>
public interface IBlingoSpriteTransitionManager : IBlingoSpriteManager<BlingoTransitionSprite>
{
    BlingoTransitionSprite Add(int frameNumber, BlingoTransitionFrameSettings? settings = null);
    BlingoTransitionSprite Add(int begin, BlingoTransitionMember transitionMember);
    BlingoTransitionSprite? GetFrameSprite(int frame);
}
internal class BlingoSpriteTransitionManager : BlingoSpriteManager<BlingoTransitionSprite>, IBlingoSpriteTransitionManager
{

    public BlingoSpriteTransitionManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoTransitionSprite.SpriteNumOffset, movie, environment)
    {
    }

    protected override BlingoTransitionSprite OnCreateSprite(BlingoMovie movie, Action<BlingoTransitionSprite> onRemove)
        => new BlingoTransitionSprite(_environment.Events, _environment.CastLibsContainer.ActiveCast, onRemove);

    public BlingoTransitionSprite Add(int frameNumber, BlingoTransitionFrameSettings? settings = null)
    {
        var sprite = AddSprite(1, "TransitionChange_" + frameNumber);
        sprite.BeginFrame = frameNumber;
        sprite.EndFrame = frameNumber;
        if (settings != null)
            sprite.SetSettings(settings);

        return sprite;
    }

    public BlingoTransitionSprite Add(int begin, BlingoTransitionMember transitionMember)
    {
        var sprite = AddSprite(begin);
        sprite.SetMember(transitionMember);
        return sprite;
    }

    public BlingoTransitionSprite? GetFrameSprite(int frame)
        => _allTimeSprites.FirstOrDefault(t => t.BeginFrame == frame);

    protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
    {
        var sprite = Add(begin);
        if (member is BlingoTransitionMember memberTyped)
            sprite.SetMember(memberTyped);
        return sprite;
    }
}

