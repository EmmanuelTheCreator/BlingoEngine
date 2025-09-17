using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites.Events;

namespace BlingoEngine.Sprites
{
    /// <summary>
    /// Represents an internal sprite actor that participates in the begin/step/end sprite lifecycle.
    /// </summary>
    internal interface IPlayableActor : IHasBeginSpriteEvent, IHasStepFrameEvent, IHasEndSpriteEvent
    {
    }
}

