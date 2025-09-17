using AbstUI.Inputs;
using BlingoEngine.Events;

namespace BlingoEngine.Inputs
{
    /// <summary>
    /// Lingo Key Event Handler interface.
    /// </summary>
    public interface IBlingoKeyEventHandler : IAbstKeyEventHandler<BlingoKeyEvent>
    {
        new void RaiseKeyDown(BlingoKeyEvent blingoKey);
        new void RaiseKeyUp(BlingoKeyEvent blingoKey);
    }
}


