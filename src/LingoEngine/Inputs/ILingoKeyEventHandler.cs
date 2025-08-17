using AbstUI.Inputs;
using LingoEngine.Events;

namespace LingoEngine.Inputs
{
    public interface ILingoKeyEventHandler : IAbstKeyEventHandler<LingoKeyEvent>
    {
        new void RaiseKeyDown(LingoKeyEvent lingoKey);
        new void RaiseKeyUp(LingoKeyEvent lingoKey);
    }
}

