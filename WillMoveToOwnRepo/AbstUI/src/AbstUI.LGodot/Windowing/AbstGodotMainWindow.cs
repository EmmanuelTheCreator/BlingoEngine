using Godot;
using AbstUI.Primitives;
using AbstUI.Windowing;

namespace AbstUI.LGodot.Windowing;

public class AbstGodotMainWindow : IAbstFrameworkMainWindow
{
    private readonly IAbstGodotRootNode _root;
    private AbstMainWindow _window = null!;

    public AbstGodotMainWindow(IAbstGodotRootNode root)
    {
        _root = root;
        var viewport = root.RootNode.GetViewport();
        if (viewport != null)
        {
            viewport.SizeChanged += OnViewportSizeChanged;
        }
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            DisplayServer.WindowSetTitle(value);
        }
    }

    public void Init(AbstMainWindow instance)
    {
        _window = instance;
        _window.SetTheSizeFromFW(GetTheSize());
    }

    private void OnViewportSizeChanged()
    {
        _window.SetTheSizeFromFW(GetTheSize());
    }

    public APoint GetTheSize()
    {
        var size = _root.RootNode.GetViewport().GetVisibleRect().Size;
        return new APoint(size.X, size.Y);
    }

    public void SetTheSize(APoint size)
    {
        DisplayServer.WindowSetSize(new Vector2I((int)size.X, (int)size.Y));
    }
}

