using Godot;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Gfx;

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

    public void ShowConfirmDialog(string title, string message, Action<bool> onResult)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return;

        var dialog = new ConfirmationDialog
        {
            Title = title,
            DialogText = message
        };

        dialog.Confirmed += () => { onResult(true); dialog.QueueFree(); };
        dialog.Canceled += () => { onResult(false); dialog.QueueFree(); };

        root.AddChild(dialog);
        dialog.PopupCentered();
    }

    public void ShowCustomDialog(string title, ILingoFrameworkGfxPanel panel)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return;

        if (panel is not Node node)
            throw new ArgumentException("Panel must be a Godot node", nameof(panel));

        var dialog = new AcceptDialog
        {
            Title = title
        };
        dialog.GetOkButton().Text = "Close";
        dialog.Confirmed += () => dialog.QueueFree();
        dialog.Canceled += () => dialog.QueueFree();

        dialog.AddChild(node);

        root.AddChild(dialog);
        dialog.PopupCentered();
    }

    public void ShowNotification(string message)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return;

        var panel = new Panel
        {
            CustomMinimumSize = new Vector2(200, 40)
        };
        var style = new StyleBoxFlat { BgColor = new Color(1f, 1f, 0.8f) };
        panel.AddThemeStyleboxOverride("panel", style);

        var label = new Label { Text = message };
        label.Position = new Vector2(5, 5);
        panel.AddChild(label);

        root.AddChild(panel);
        panel.Position = new Vector2(root.Size.X - panel.CustomMinimumSize.X - 10, 10);

        var timer = new Timer { WaitTime = 5, OneShot = true };
        timer.Timeout += () => panel.QueueFree();
        panel.AddChild(timer);
        timer.Start();
    }

    private void SetTheActiveWindow(BaseGodotWindow window)
    {
        if (ActiveWindow != null)
        {
            ActiveWindow.ZIndex = ZIndexInactiveWindow;
            ActiveWindow.QueueRedraw();
        }
        ActiveWindow = window;
        ActiveWindow.ZIndex = 0;
        var parent = window.GetParent();
        if (parent != null)
            parent.MoveChild(window, parent.GetChildCount() - 1);
        window.GrabFocus();
        window.QueueRedraw();
    }
}
