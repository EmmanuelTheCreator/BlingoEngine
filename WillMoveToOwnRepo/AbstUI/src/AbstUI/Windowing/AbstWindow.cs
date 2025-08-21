using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Windowing;

public interface IAbstWindowInternal : IAbstWindow, IAbstNode, IDisposable
{
    void ResizeFromFW(bool firstLoad, int width, int height);
    void RaiseWindowStateChanged(bool v);
    void SetPositionFromFW(int x, int y);
}
public class AbstWindow<TFrameworkWindow> : IAbstWindow, IDisposable, IAbstKeyEventHandler<AbstKeyEvent>, IAbstWindowInternal
    where TFrameworkWindow : IAbstFrameworkWindow, IAbstFrameworkNode
{
    protected readonly IAbstComponentFactory _componentFactory;
    protected TFrameworkWindow _framework;
    public IAbstMouse Mouse { get;  }
    public IAbstMouse<AbstMouseEvent> MouseT => (IAbstMouse<AbstMouseEvent>)Mouse;
    public IAbstKey Key { get; }

    public TFrameworkWindow Framework => _framework;
    public IAbstFrameworkWindow FrameworkObj => _framework;

    IAbstFrameworkNode IAbstNode.FrameworkObj => _framework;
    T IAbstNode.Framework<T>() => (T)(IAbstFrameworkNode)_framework;

    public int WindowTitleHeight { get; set; }
    public string WindowCode { get; }
    public string Title { get => _framework.Title; set => _framework.Title = value; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int MinimumWidth { get; set; }
    public int MinimumHeight { get; set; }
    public APoint Position => _framework.GetPosition();
    public APoint Size => _framework.GetSize();
    public bool IsOpen => _framework.IsOpen;
    public bool IsActiveWindow => _framework.IsActiveWindow;

    public ARect MouseOffset => ARect.New(X, Y+ WindowTitleHeight, Width, Height);

    public bool IsActivated { get; internal set; }
    public string Name { get => WindowCode; set { } }
    public bool Visibility { get => IsOpen; set { } }
    float IAbstNode.Width { get => Width; set => Width = (int)value; }
    float IAbstNode.Height { get => Height; set => Height = (int)value; }
    public AMargin Margin { get; set; } = new AMargin();

    

    public event Action<bool>? OnWindowStateChanged;
    public event Action<float, float>? OnResize;

#pragma warning disable CS8618
    public AbstWindow(IServiceProvider serviceProvider, string windowCode)
#pragma warning restore CS8618 
    {
        WindowCode = windowCode;
        _componentFactory = serviceProvider.GetRequiredService<IAbstComponentFactory>();
        Mouse = serviceProvider.GetRequiredService<IAbstGlobalMouse>().CreateNewInstance(this);
        Key = serviceProvider.GetRequiredService<IAbstGlobalKey>().CreateNewInstance(this);
        Key.Subscribe(this);
    }
    public virtual void Dispose()
    {
        Key.Unsubscribe(this);
    }
    public virtual void Init(IAbstFrameworkWindow frameworkWindow)
    {
        _framework = (TFrameworkWindow)frameworkWindow;
    }


    public virtual void OpenWindow() => _framework.OpenWindow();
    public virtual void CloseWindow() => _framework.CloseWindow();
    public virtual void MoveWindow(int x, int y) => _framework.MoveWindow(x, y);
    public virtual void SetPositionAndSize(int x, int y, int width, int height)
        => _framework.SetPositionAndSize(x, y, width, height);
    public virtual void SetSize(int width, int height)
        => _framework.SetSize(width, height);

   



    public void RaiseKeyDown(AbstKeyEvent lingoKey)
    {
        if (IsActiveWindow)
            OnRaiseKeyDown(lingoKey);
    }

    public void RaiseKeyUp(AbstKeyEvent lingoKey)
    {
        if (IsActiveWindow)
            OnRaiseKeyUp(lingoKey);
    }

    protected virtual void OnRaiseKeyDown(AbstKeyEvent lingoKey) { }

    protected virtual void OnRaiseKeyUp(AbstKeyEvent lingoKey) { }

    public (float X, float Y) MouseGetAbolutePosition() => (Mouse.MouseH + Position.X, Mouse.MouseV + Position.Y);

    /// <summary>
    /// Creates a new context menu for this window. Don't forget to dispose.
    /// </summary>
    protected AbstContextMenu CreateContextMenu(Func<bool>? isllowed = null)
    {
        if (_framework == null) throw new Exception("Context menu can only be created once the framework object has been set. Call in it the Init method of the DirectorWindow.");
        var contextMenu = new AbstContextMenu(_framework, _componentFactory, MouseGetAbolutePosition, isllowed ?? AllowContextMenu, Mouse as AbstMouse);
        return contextMenu;
    }
    protected virtual bool AllowContextMenu() => IsActiveWindow;

    public void ResizeFromFW(bool firstLoad, int width, int height)
    {
        if (width < MinimumWidth) width = MinimumWidth;
        if (height < MinimumHeight) height = MinimumHeight;
        OnResizing(firstLoad, width, height);
        OnResize?.Invoke(width, height);
    }
    protected virtual void OnResizing(bool firstLoad, int width, int height) 
    {
        Width = width;
        Height = height;
        
    }

    void IAbstWindow.SetActivated(bool state) => IsActivated = state;

    public void RaiseWindowStateChanged(bool state) => OnWindowStateChanged?.Invoke(state);

    public void SetPositionFromFW(int x, int y)
    {
        X = x;
        Y = y;
    }

    
}
