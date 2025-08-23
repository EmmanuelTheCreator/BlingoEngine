using System.Collections.Generic;

namespace LingoEngine.Transitions.TransitionLibrary;

public class LingoTransitionLibrary : ILingoTransitionLibrary
{
    private readonly Dictionary<int, LingoBaseTransition> _transitions =
        new() { { 1, new FadeTransition() } };

    public LingoBaseTransition Get(int id)
    {
        if (_transitions.TryGetValue(id, out var transition))
            return transition;
        return _transitions[1];
    }

    public IEnumerable<LingoBaseTransition> GetAll() => _transitions.Values;
}

