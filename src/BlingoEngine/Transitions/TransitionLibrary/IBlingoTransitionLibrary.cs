using System.Collections.Generic;

namespace BlingoEngine.Transitions.TransitionLibrary;

public interface IBlingoTransitionLibrary
{
    BlingoBaseTransition Get(int id);
    IEnumerable<BlingoBaseTransition> GetAll();
}


