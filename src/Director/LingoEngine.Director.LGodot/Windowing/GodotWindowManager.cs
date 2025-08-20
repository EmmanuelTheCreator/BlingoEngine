using AbstEngine.Director.LGodot;
using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Windowing;
using LingoEngine.Director.LGodot.Movies;

internal class DirGodotWindowManager : AbstGodotWindowManager
{
    public DirGodotWindowManager(IAbstWindowManager directorWindowManager, IAbstGodotStyleManager lingoGodotStyleManager, IAbstComponentFactory frameworkFactory, IAbstGodotRootNode godotRootNode) : base(directorWindowManager, lingoGodotStyleManager, frameworkFactory, godotRootNode)
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
