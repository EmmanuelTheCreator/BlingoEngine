using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public abstract class AbstBlazorComponentModelBase : IAbstFrameworkLayoutNode
{
    public event Action? Changed;
    protected void RaiseChanged() => Changed?.Invoke();

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; RaiseChanged(); } }
    }

    private bool _visibility = true;
    public bool Visibility
    {
        get => _visibility;
        set { if (_visibility != value) { _visibility = value; RaiseChanged(); } }
    }

    private float _width;
    public float Width
    {
        get => _width;
        set { if (Math.Abs(_width - value) > float.Epsilon) { _width = value; RaiseChanged(); } }
    }

    private float _height;
    public float Height
    {
        get => _height;
        set { if (Math.Abs(_height - value) > float.Epsilon) { _height = value; RaiseChanged(); } }
    }

    private AMargin _margin;
    public AMargin Margin
    {
        get => _margin;
        set { _margin = value; RaiseChanged(); }
    }

    private float _x;
    public float X
    {
        get => _x;
        set { if (Math.Abs(_x - value) > float.Epsilon) { _x = value; RaiseChanged(); } }
    }

    private float _y;
    public float Y
    {
        get => _y;
        set { if (Math.Abs(_y - value) > float.Epsilon) { _y = value; RaiseChanged(); } }
    }

    public object FrameworkNode => this;

    public int ZIndex { get; set; }

    public virtual void Dispose() { }
}
