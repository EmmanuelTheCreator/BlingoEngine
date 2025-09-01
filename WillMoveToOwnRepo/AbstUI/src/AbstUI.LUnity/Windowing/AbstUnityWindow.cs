using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.LUnity.Components.Containers;
using AbstUI.Inputs;

namespace AbstUI.LUnity.Windowing;

/// <summary>
/// Basic Unity implementation of <see cref="IAbstFrameworkWindow"/>. This is a light-weight
/// wrapper around <see cref="AbstUnityPanel"/> that allows the core window system to
/// manage windows even though Unity itself does not provide native multiple windows.
/// </summary>
internal class AbstUnityWindow : AbstUnityPanel, IAbstFrameworkWindow
{
    private IAbstWindow _window = null!;
    private IAbstWindowInternal _internalWindow = null!;
    private string _title = string.Empty;
    private IAbstFrameworkNode? _content;

    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }
    public AColor BackgroundTitleColor { get; set; }
    public bool IsActiveWindow => Visibility;
    public bool IsOpen => Visibility;

    public IAbstMouse Mouse => _window.Mouse;
    public IAbstKey AbstKey => _window.Key;

    public IAbstFrameworkNode? Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            RemoveAll();
            _content = value;
            if (value is IAbstFrameworkLayoutNode layout)
                AddItem(layout);
        }
    }

    public void Init(IAbstWindow instance)
    {
        _window = instance;
        _internalWindow = (IAbstWindowInternal)instance;
        instance.Init(this);
    }

    public void OpenWindow() => Visibility = true;
    public void CloseWindow() => Visibility = false;

    public void MoveWindow(int x, int y)
    {
        X = x;
        Y = y;
        _internalWindow.SetPositionFromFW(x, y);
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        MoveWindow(x, y);
        SetSize(width, height);
    }

    public APoint GetPosition() => new(X, Y);
    public APoint GetSize() => new(Width, Height);
    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        _internalWindow.ResizeFromFW(false, width, height);
    }
}
