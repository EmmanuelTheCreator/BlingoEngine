using Godot;
using AbstUI.Components;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.LGodot.Movies;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Primitives;

namespace LingoEngine.Director.LGodot.Windowing;

public interface IDirGodotWindowManager : IDirFrameworkWindowManager
{
    void Register(BaseGodotWindow godotWindow);
    void SetActiveWindow(BaseGodotWindow window, Vector2 mousePoint);
   

    BaseGodotWindow? ActiveWindow { get; }
}
internal class DirGodotWindowManager : IDirGodotWindowManager
{
    public const int ZIndexInactiveWindow = -1000;
    public const int ZIndexInactiveWindowStage = -4000;
    private IDirectorWindowManager _directorWindowManager;
    private readonly IAbstGodotStyleManager _lingoGodotStyleManager;
    private readonly Dictionary<string, BaseGodotWindow> _godotWindows = new();
    public BaseGodotWindow? ActiveWindow { get; private set; }

    public DirGodotWindowManager(IDirectorWindowManager directorWindowManager, IAbstGodotStyleManager lingoGodotStyleManager)
    {
        _directorWindowManager = directorWindowManager;
        _lingoGodotStyleManager = lingoGodotStyleManager;
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

    public void SetActiveWindow(IDirectorWindowRegistration windowRegistration)
    {
        var window = _godotWindows[windowRegistration.WindowCode];
        SetTheActiveWindow(window);
    }
    private void SetTheActiveWindow(BaseGodotWindow window)
    {
        if (ActiveWindow != null)
        {
            ActiveWindow.ZIndex = ActiveWindow is DirGodotStageWindow? ZIndexInactiveWindowStage : ZIndexInactiveWindow;
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

    #endregion


    public IDirectorWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
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
        return new DirectorWindowDialogReference(dialog.QueueFree);
    }

    public IDirectorWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
    {
        var root = ActiveWindow?.GetTree().Root;
        if (root == null)
            return null;

        if (panel is not Panel node)
            throw new ArgumentException("Panel must be a Godot node", nameof(panel));

        // Set background color
        var styleBox = new StyleBoxFlat
        {
            BgColor = DirectorColors.PopupWindow_Background.ToGodotColor()
        };
        node.AddThemeStyleboxOverride("panel", styleBox);


        var dialog = new Window
        {
            Title = title,
            //AlwaysOnTop = true, // <- blocks combo boxes
            Exclusive = true,
            PopupWindow = true,
            Unresizable = true,
            Size = new Vector2I((int)panel.Width, (int)panel.Height),
            Theme = _lingoGodotStyleManager.GetTheme(AbstGodotThemeElementType.PopupWindow),
        };
        ReplaceIconColor(dialog, "close", new Color("#777777"));
        ReplaceIconColor(dialog, "close_hl", Colors.Black);
        ReplaceIconColor(dialog, "close_pressed", Colors.Black);
        root.AddChild(dialog);
        dialog.CloseRequested += dialog.QueueFree;
        dialog.AddChild(node);
        dialog.PopupCentered();


        return new DirectorWindowDialogReference(dialog.QueueFree);
    }

    private static void ReplaceIconColor(Window dialog, string name, Color colorNew)
    {
        var closeButton = dialog.GetThemeIcon(name);
        ImageTexture tinted = CreateTintedIcon(closeButton, colorNew);
        dialog.AddThemeIconOverride(name, tinted);
    }

    private static ImageTexture CreateTintedIcon(Texture2D closeButton, Color colorNew)
    {
        var image = closeButton.GetImage();
        image.Convert(Image.Format.Rgba8);

        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                var color = image.GetPixel(x, y);
                color = new Color(colorNew.R, colorNew.G, colorNew.B, color.A); // tint red
                image.SetPixel(x, y, color);
            }
        }

        var tinted = ImageTexture.CreateFromImage(image);

        return tinted;
    }

    public IDirectorWindowDialogReference? ShowNotification(string message, DirUINotificationType type)
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
            DirUINotificationType.Error => (
                DirectorColors.Notification_Error_Bg.ToGodotColor(),
                DirectorColors.Notification_Error_Border.ToGodotColor()
            ),
            DirUINotificationType.Information => (
                DirectorColors.Notification_Info_Bg.ToGodotColor(),
                DirectorColors.Notification_Info_Border.ToGodotColor()
            ),
            _ => (
                DirectorColors.Notification_Warning_Bg.ToGodotColor(),
                DirectorColors.Notification_Warning_Border.ToGodotColor()
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
        return new DirectorWindowDialogReference(panel.QueueFree);
    }

    
}
