using LingoEngine.Director.Core.Windowing;

namespace LingoEngine.Director.LGodot.Windowing;

public interface IDirGodotWindowManager : IDirFrameworkWindowManager
{
    void Register(BaseGodotWindow godotWindow);
    void SetActiveWindow(BaseGodotWindow window);
    BaseGodotWindow? ActiveWindow { get; }
}
internal class DirGodotWindowManager : IDirGodotWindowManager
{
    public const int ZIndexInactiveWindow = -4000;
    private IDirectorWindowManager _directorWindowManager;
    private readonly Dictionary<string, BaseGodotWindow> _godotWindows = new();
    public BaseGodotWindow? ActiveWindow { get; private set; }

    public DirGodotWindowManager(IDirectorWindowManager directorWindowManager)
    {
        _directorWindowManager = directorWindowManager;
        directorWindowManager.Init(this);
    }
    public void Register(BaseGodotWindow godotWindow)
    {
        _godotWindows.Add(godotWindow.WindowCode, godotWindow);
        godotWindow.ZIndex = ZIndexInactiveWindow;
    }

    public void SetActiveWindow(BaseGodotWindow window)
    {
        _directorWindowManager.SetActiveWindow(window.WindowCode);
        if (ActiveWindow == window)
        {
            window.GrabFocus();
            return;
        }
        SetTheActiveWindow(window);
    }

   

    public void SetActiveWindow(IDirectorWindowRegistration windowRegistration)
    {
        var window = _godotWindows[windowRegistration.WindowCode];
        SetTheActiveWindow(window);
    }

    private void SetTheActiveWindow(BaseGodotWindow window)
    {
        if (ActiveWindow != null)
            ActiveWindow.ZIndex = ZIndexInactiveWindow;
        ActiveWindow = window;
        ActiveWindow.ZIndex = 0;
        var parent = window.GetParent();
        if (parent != null)
            parent.MoveChild(window, parent.GetChildCount() - 1);
        window.GrabFocus();
    }
}
