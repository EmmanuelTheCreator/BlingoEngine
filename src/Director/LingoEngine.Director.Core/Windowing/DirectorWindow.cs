using LingoEngine.AbstUI.Primitives;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;

namespace LingoEngine.Director.Core.Windowing;

public class DirectorWindow<TFrameworkWindow> : IDirectorWindow, IDisposable, ILingoKeyEventHandler
    where TFrameworkWindow : IDirFrameworkWindow
{
#pragma warning disable CS8618
    private TFrameworkWindow _Framework;

    public TFrameworkWindow Framework => _Framework;
    protected ILingoMouse Mouse { get; private set; }

    protected readonly ILingoFrameworkFactory _factory;
#pragma warning restore CS8618

    protected LingoKey LingoKey { get; }

#pragma warning disable CS8618 
    public DirectorWindow(ILingoFrameworkFactory factory)
#pragma warning restore CS8618 
    {
        _factory = factory;
        LingoKey = factory.CreateKey();
        LingoKey.Subscribe(this);
    }

    public virtual void Init(IDirFrameworkWindow frameworkWindow)
    {
        _Framework = (TFrameworkWindow)frameworkWindow;
        Mouse = frameworkWindow.Mouse;
    }
    public APoint Position => _Framework.GetPosition();
    public APoint Size => _Framework.GetSize();
    public bool IsOpen => _Framework.IsOpen;
    public bool IsActiveWindow => _Framework.IsActiveWindow;
    public virtual void OpenWindow() => _Framework.OpenWindow();
    public virtual void CloseWindow() => _Framework.CloseWindow();
    public virtual void MoveWindow(int x, int y) => _Framework.MoveWindow(x, y);
    public virtual void SetPositionAndSize(int x, int y, int width, int height)
        => _Framework.SetPositionAndSize(x, y, width, height);
    public virtual void SetSize(int width, int height)
        => _Framework.SetSize(width, height);

    public virtual void Dispose()
    {
        LingoKey.Unsubscribe(this);

    }

    public IDirFrameworkWindow FrameworkObj => _Framework;

    public void RaiseKeyDown(LingoKey lingoKey)
    {
        if (IsActiveWindow)
            OnRaiseKeyDown(lingoKey);
    }

    public void RaiseKeyUp(LingoKey lingoKey)
    {
        if (IsActiveWindow)
            OnRaiseKeyUp(lingoKey);
    }

    protected virtual void OnRaiseKeyDown(LingoKey lingoKey) { }

    protected virtual void OnRaiseKeyUp(LingoKey lingoKey) { }

    public (float X, float Y) MouseGetAbolutePosition() => (Mouse.MouseH + Position.X, Mouse.MouseV + Position.Y);

    /// <summary>
    /// Creates a new context menu for this window. Don't forget to dispose.
    /// </summary>
    protected DirContextMenu CreateContextMenu(Func<bool>? isllowed = null)
    {
        if (_Framework == null) throw new Exception("Context menu can only be created once the framework object has been set. Call in it the Init method of the DirectorWindow.");
        var contextMenu = new DirContextMenu(_Framework, _factory, MouseGetAbolutePosition, isllowed ?? AllowContextMenu, Mouse as LingoMouse);
        return contextMenu;
    }
    protected virtual bool AllowContextMenu() => IsActiveWindow;
}
