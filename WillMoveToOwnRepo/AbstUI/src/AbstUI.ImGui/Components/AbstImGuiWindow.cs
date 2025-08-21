using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components;

internal class AbstImGuiWindow : AbstImGuiPanel, IAbstFrameworkWindow, IDisposable
{
    private readonly AbstImGuiComponentFactory _factory;
    private readonly AbstWindow _lingoWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;

    public AbstImGuiWindow(AbstWindow window, AbstImGuiComponentFactory factory) : base(factory)
    {
        _lingoWindow = window;
        _factory = factory;
        var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        _lingoWindow.Init(this, mouse, key);
        Visibility = false;
    }


    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public new float Width
    {
        get => base.Width;
        set
        {
            if (Math.Abs(base.Width - value) > float.Epsilon)
                base.Width = value;
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            if (Math.Abs(base.Height - value) > float.Epsilon)
                base.Height = value;
        }
    }

    public bool IsPopup
    {
        get => _isPopup;
        set => _isPopup = value;
    }

    public bool Borderless
    {
        get => _borderless;
        set => _borderless = value;
    }

    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }

    private IAbstFrameworkNode? _content;
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
            _lingoWindow.SetContentFromFW(value);
        }
    }


    public void OnResize(int width, int height)
    {
        // update local size so layout information stays in sync with the native window
        Width = width;
        Height = height;

        // notify underlying window abstraction
        _lingoWindow.Resize(width, height);
    }

    public void Popup()
    {
        _factory.RootContext.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void PopupCentered()
    {
        APoint size = _factory.RootContext.GetWindowSize();

        X = (size.X - Width) / 2f;
        Y = (size.Y - Height) / 2f;
        Popup();
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void Hide()
    {
        Visibility = false;
        _factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _lingoWindow.RaiseWindowStateChanged(false);
    }
}
