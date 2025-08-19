using AbstUI;
using AbstUI.Components;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;
using LingoEngine.LGodot;

namespace AbstEngine.Director.LGodot.Windowing;

public interface IAbstGodotWindowManager : IAbstFrameworkWindowManager
{
    void Register(BaseGodotWindow godotWindow);
    void SetActiveWindow(BaseGodotWindow window, Vector2 mousePoint);


    BaseGodotWindow? ActiveWindow { get; }
}
public class AbstGodotWindowManager : IAbstGodotWindowManager
{
    public const int ZIndexInactiveWindow = -1000;
    public const int ZIndexInactiveWindowStage = -4000;
    private IAbstWindowManager _directorWindowManager;
    private readonly IAbstGodotStyleManager _lingoGodotStyleManager;
    private readonly IAbstComponentFactory _frameworkFactory;
    private readonly Dictionary<string, BaseGodotWindow> _godotWindows = new();
    public BaseGodotWindow? ActiveWindow { get; private set; }

    public AbstGodotWindowManager(IAbstWindowManager directorWindowManager, IAbstGodotStyleManager lingoGodotStyleManager, IAbstComponentFactory frameworkFactory)
    {
        _directorWindowManager = directorWindowManager;
        _lingoGodotStyleManager = lingoGodotStyleManager;
        _frameworkFactory = frameworkFactory;
        directorWindowManager.Init(this);
    }
    public void Register(BaseGodotWindow godotWindow)
    {
        _godotWindows.Add(godotWindow.WindowCode, godotWindow);
        godotWindow.ZIndex = ZIndexInactiveWindow;
    }

    #region Window activation
    public void SetActiveWindow(BaseGodotWindow window, Vector2 mousePoint)
    {
        if (ActiveWindow == window)
            return;
        if (ActiveWindow != null && ActiveWindow.GetGlobalRect().HasPoint(mousePoint))
        {
            // if the active window is clicked, we do not change the active window
            // this is to prevent flickering when clicking on the active window
            return;
        }

        SetTheActiveWindow(window);
    }


    private BaseGodotWindow? GetTopMostWindow(Vector2 mouse)
    {
        return _godotWindows.Values
                    .Where(w => w.Visible && w.GetGlobalRect().HasPoint(mouse))
                    .OrderByDescending(w => w.ZIndex)
                    .FirstOrDefault();
    }

    public void SetActiveWindow(IAbstWindowRegistration windowRegistration)
    {
        var window = _godotWindows[windowRegistration.WindowCode];
        SetTheActiveWindow(window);
    }
    protected virtual void SetTheActiveWindow(BaseGodotWindow window)
    {
        ActiveWindow = window;
        ActiveWindow.ZIndex = 0;
        var parent = window.GetParent();
        if (parent != null)
            parent.MoveChild(window, parent.GetChildCount() - 1);
        window.GrabFocus();
        window.QueueRedraw();
    }

    #endregion


    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return null;

        var dialog = new ConfirmationDialog
        {
            Title = title,
            DialogText = message,
            Theme = _lingoGodotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow),
        };

        dialog.Confirmed += () => { onResult(true); dialog.QueueFree(); };
        dialog.Canceled += () => { onResult(false); dialog.QueueFree(); };

        root.AddChild(dialog);
        dialog.PopupCentered();
        return new AbstWindowDialogReference(dialog.QueueFree);
    }

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return null;

        if (panel is not Panel node)
            throw new ArgumentException("Panel must be a Godot node", nameof(panel));

        // Set background color
        var styleBox = new StyleBoxFlat
        {
            BgColor = AbstDefaultColors.PopupWindow_Background.ToGodotColor()
        };
        node.AddThemeStyleboxOverride("panel", styleBox);

        var dialogAbst = _frameworkFactory.GetRequiredService<IAbstDialog>();
        var dialog = dialogAbst.FrameworkObj<AbstGodotDialog>();
        dialog.Title = title;
        dialog.Size = new Vector2I((int)panel.Width, (int)panel.Height);
        dialog.Theme = _lingoGodotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow);
        root.AddChild(dialog);
        dialog.CloseRequested += dialog.QueueFree;
        dialog.AddChild(node);
        dialog.PopupCentered();

        return new AbstWindowDialogReference(dialog.QueueFree, dialog);
    }

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? lingoDialog = null)
        where TDialog : class, IAbstDialog
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return null;

        if (panel is not Panel node)
            throw new ArgumentException("Panel must be a Godot node", nameof(panel));

        // Set background color
        var styleBox = new StyleBoxFlat
        {
            BgColor = AbstDefaultColors.PopupWindow_Background.ToGodotColor()
        };
        node.AddThemeStyleboxOverride("panel", styleBox);
        AbstGodotDialog dialog;
        if (lingoDialog != null)
            dialog = lingoDialog.FrameworkObj<AbstGodotDialog>();
        else
        {
            lingoDialog = _frameworkFactory.CreateElement<TDialog>();
            dialog = lingoDialog.FrameworkObj<AbstGodotDialog>();
        }

        dialog.Title = title;
        
        dialog.Size = new Vector2I((int)panel.Width, (int)panel.Height);
        dialog.Theme = _lingoGodotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow);
       
        root.AddChild(dialog);
        dialog.CloseRequested += dialog.QueueFree;
        dialog.AddChild(node);
        if (lingoDialog != null)
        {
            var frameworkDialog = (IAbstFrameworkDialog)dialog;
            frameworkDialog.Init(lingoDialog);
            lingoDialog.Init(frameworkDialog);
        }
        dialog.PopupCentered();

        return new AbstWindowDialogReference( dialog.QueueFree, dialog);
    }

    
   

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return null;

        var panel = new Panel
        {
            CustomMinimumSize = new Vector2(200, 40)
        };
        var (bg, border) = type switch
        {
            AbstUINotificationType.Error => (
                AbstDefaultColors.Notification_Error_Bg.ToGodotColor(),
                AbstDefaultColors.Notification_Error_Border.ToGodotColor()
            ),
            AbstUINotificationType.Info => (
                AbstDefaultColors.Notification_Info_Bg.ToGodotColor(),
                AbstDefaultColors.Notification_Info_Border.ToGodotColor()
            ),
            _ => (
                AbstDefaultColors.Notification_Warning_Bg.ToGodotColor(),
                AbstDefaultColors.Notification_Warning_Border.ToGodotColor()
            ),
        };
        var style = new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var label = new Label { Text = message };
        label.Position = new Vector2(5, 5);
        panel.AddChild(label);

        root.AddChild(panel);
        panel.Position = new Vector2(root.Size.X - panel.CustomMinimumSize.X - 10, 10);

        var timer = new Godot.Timer { WaitTime = 5, OneShot = true };
        timer.Timeout += panel.QueueFree;
        panel.AddChild(timer);
        timer.Start();
        return new AbstWindowDialogReference(panel.QueueFree);
    }


}
