using LingoEngine.Movies.Events;

namespace LingoEngine.Core
{
    public class ActorList
    {
        private List<IHasStepFrameEvent> _actors = new();
        public void Add(IHasStepFrameEvent actor) => _actors.Add(actor);
        public void Remove(IHasStepFrameEvent actor) => _actors.Remove(actor);

        internal void Invoke()
        {
            foreach (var actor in _actors.ToArray()) // make a copy of the array so that it can be modified during scripts.
                actor.StepFrame();
        }
    }
}
