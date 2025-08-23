using System.Collections.Generic;

namespace LingoEngine.Transitions.TransitionLibrary;

public interface ILingoTransitionLibrary
{
    LingoBaseTransition Get(int id);
    IEnumerable<LingoBaseTransition> GetAll();
}

