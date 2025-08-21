using AbstEngine.Director.LGodot;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;

namespace AbstUI.LGodot.Windowing;



public interface IAbstGodotWindowManager : IAbstFrameworkWindowManager
{
    BaseGodotWindow? ActiveWindow { get; }
    Control RootNode { get; }
}
public class AbstGodotWindowManager : IAbstGodotWindowManager , IDisposable
{
    protected BaseGodotWindow? _lastActiveWindow;
    private IAbstWindowManager _windowManager;
    private readonly IAbstGodotStyleManager _godotStyleManager;
    private readonly IAbstComponentFactory _frameworkFactory;

    public Control RootNode { get; }

    private readonly Dictionary<string, Lazy<BaseGodotWindow>> _godotWindows = new();

    public BaseGodotWindow? ActiveWindow => _windowManager.ActiveWindow?.FrameworkObj as BaseGodotWindow;

    public AbstGodotWindowManager(IAbstWindowManager windowManager, IAbstGodotStyleManager godotStyleManager, IAbstComponentFactory frameworkFactory, IAbstGodotRootNode abstGodotRootNode)
    {
        _windowManager = windowManager;
        _windowManager.NewWindowCreated += NewWindowCreated;
        _godotStyleManager = godotStyleManager;
        _frameworkFactory = frameworkFactory;
        RootNode = new Control();
        RootNode.Name = "LingoGodotRootNode";
        abstGodotRootNode.RootNode.AddChild(RootNode);
        windowManager.Init(this);
    }
    public void Dispose()
    {
        _windowManager.NewWindowCreated -= NewWindowCreated;
        _godotWindows.Clear();
    }

    private void NewWindowCreated(IAbstWindow window)
    {
        _godotWindows.Add(window.WindowCode, new Lazy<BaseGodotWindow>(() => (BaseGodotWindow)window.FrameworkObj));
        RootNode.AddChild(_godotWindows[window.WindowCode].Value);
    }



    #region Window activation
    //public void SetActiveWindow(BaseGodotWindow window, Vector2 mousePoint)
    //{
    //    if (ActiveWindow == window)
    //        return;
    //    if (ActiveWindow != null && ActiveWindow.GetGlobalRect().HasPoint(mousePoint))
    //    {
    //        // if the active window is clicked, we do not change the active window
    //        // this is to prevent flickering when clicking on the active window
    //        return;
    //    }

    //    SetTheActiveWindow(window);
    //}


    //private BaseGodotWindow? GetTopMostWindow(Vector2 mouse)
    //{
    //    return _godotWindows.Values
    //                .Where(w => w.Value.Visible && w.Value.GetGlobalRect().HasPoint(mouse))
    //                .OrderByDescending(w => w.Value.ZIndex)
    //                .FirstOrDefault();
    //}

    public void SetActiveWindow(IAbstWindow window)
    {
        var godotWindow = _godotWindows[window.WindowCode].Value;
        SetTheActiveWindow(godotWindow);
    }
    protected virtual void SetTheActiveWindow(BaseGodotWindow window)
    {
        //if (_lastActiveWindow != null)
        window.ZIndex = 0;
        var parent = window.GetParent();
        if (parent != null)
            parent.MoveChild(window, parent.GetChildCount() - 1);
        window.GrabFocus();
        window.QueueRedraw();
        _lastActiveWindow = window; 
    }

    #endregion


    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
    {
        var root = RootNode.GetTree().Root;
        if (root == null)
            return null;

        var dialog = new ConfirmationDialog
        {
            Title = title,
            DialogText = message,
            Theme = _godotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow),
        };

        dialog.Confirmed += () => { onResult(true); dialog.QueueFree(); };
        dialog.Canceled += () => { onResult(false); dialog.QueueFree(); };

        root.AddChild(dialog);
        dialog.PopupCentered();
        return new AbstWindowDialogReference(dialog.QueueFree);
    }

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
    {
        var root = RootNode.GetTree().Root;
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

        var dialogAbst = _frameworkFactory.CreateElement<IAbstDialog>();
        var dialog = dialogAbst.FrameworkObj<AbstGodotDialog>();
        dialog.Title = title;
        dialog.Size = new Vector2I((int)panel.Width, (int)panel.Height);
        dialog.Theme = _godotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow);
        root.AddChild(dialog);
        dialog.CloseRequested += dialog.QueueFree;
        dialog.AddChild(node);
        dialog.PopupCentered();

        return new AbstWindowDialogReference(dialog.QueueFree, dialog);
    }

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? abstDialog = null)
        where TDialog : class, IAbstDialog
    {
        var root = RootNode.GetTree().Root;
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
        if (abstDialog != null)
            dialog = abstDialog.FrameworkObj<AbstGodotDialog>();
        else
        {
            abstDialog = _frameworkFactory.CreateElement<TDialog>();
            dialog = abstDialog.FrameworkObj<AbstGodotDialog>();
        }

        dialog.Title = title;
        
        dialog.Size = new Vector2I((int)panel.Width, (int)panel.Height);
        dialog.Theme = _godotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow);
       
        root.AddChild(dialog);
        dialog.CloseRequested += dialog.QueueFree;
        dialog.AddChild(node);
        //if (abstDialog != null)
        //{
        //    var frameworkDialog = (IAbstFrameworkDialog)dialog;
        //    frameworkDialog.Init(abstDialog);
        //    abstDialog.Init(frameworkDialog);
        //}
        dialog.PopupCentered();

        return new AbstWindowDialogReference( dialog.QueueFree, dialog);
    }

    
   

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
    {
        var root = RootNode.GetTree().Root;
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
