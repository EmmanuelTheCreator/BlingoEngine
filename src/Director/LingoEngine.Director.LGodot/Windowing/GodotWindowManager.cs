using AbstEngine.Director.LGodot;
using AbstEngine.Director.LGodot.Windowing;
using AbstUI;
using AbstUI.LGodot.Styles;
using AbstUI.Windowing;
using LingoEngine.Director.LGodot.Movies;

internal class DirGodotWindowManager : AbstGodotWindowManager
{
    public DirGodotWindowManager(IAbstWindowManager directorWindowManager, IAbstGodotStyleManager lingoGodotStyleManager, IAbstComponentFactory frameworkFactory) : base(directorWindowManager, lingoGodotStyleManager, frameworkFactory)
    {
    }

    protected override void SetTheActiveWindow(BaseGodotWindow window)
    {
        if (ActiveWindow != null)
        {
            ActiveWindow.ZIndex = ActiveWindow is DirGodotStageWindow ? ZIndexInactiveWindowStage : ZIndexInactiveWindow;
            ActiveWindow.QueueRedraw();
        }
        base.SetTheActiveWindow(window);
    }
}
