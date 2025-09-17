using AbstEngine.Director.LGodot;
using AbstUI.Windowing;
using BlingoEngine.Director.LGodot.Movies;

namespace BlingoEngine.Director.LGodot.Windowing
{
    internal class DirGodotWindowManagerDecorator
    {
        public const int ZIndexInactiveWindowStage = -4000;
        public const int ZIndexInactiveWindow = -1000;
        private readonly IAbstWindowManager _windowManager;

        public DirGodotWindowManagerDecorator(IAbstWindowManager windowManager)
        {
            _windowManager = windowManager;
            _windowManager.BeforeWindowActivated += OnBeforeWindowActivated;
        }

        private void OnBeforeWindowActivated(IAbstWindow? window)
        {
            var previousWindow = _windowManager.ActiveWindow;
            if (previousWindow == null)
                return;
            
            var godot = previousWindow.FrameworkObj as BaseGodotWindow;
            if (godot == null) return;
            godot.ZIndex = previousWindow is DirGodotStageWindow ? ZIndexInactiveWindowStage : ZIndexInactiveWindow;
            godot.QueueRedraw();
        }
    }
}
