using LingoEngine.Inputs;
using static LingoEngine.Inputs.LingoJoystickKeyboard;

namespace LingoEngine.FrameworkCommunication
{
    public static class ILingoFrameworkFactoryExtensions
    {
        public static LingoJoystickKeyboard CreateKeyboard(this ILingoFrameworkFactory factory, LingoKeyboardLayoutType layoutType = LingoKeyboardLayoutType.Azerty, bool showEscapeKey = false) 
            => new LingoJoystickKeyboard(factory);
    }
}
