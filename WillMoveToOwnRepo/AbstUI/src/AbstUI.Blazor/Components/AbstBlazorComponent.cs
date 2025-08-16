using System;

namespace AbstUI.Blazor.Components;

/// <summary>
/// Base class for Blazor GFX components providing common geometry and visibility
/// handling that synchronizes with the <see cref="AbstBlazorComponentContext"/>.
/// </summary>
public abstract class AbstBlazorComponent : IAbstBlazorComponent, IDisposable
{
    protected AbstBlazorComponent(AbstBlazorComponentFactory factory)
    {
        ComponentContext = factory.CreateContext(this);
        ComponentContext.Visible = _visibility;
    }

    public AbstBlazorComponentContext ComponentContext { get; }

    private float _x;
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            ComponentContext.X = (int)value;
        }
    }

    private float _y;
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            ComponentContext.Y = (int)value;
        }
    }

    private float _width;
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            ComponentContext.TargetWidth = (int)value;
        }
    }

    private float _height;
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            ComponentContext.TargetHeight = (int)value;
        }
    }

    private bool _visibility = true;
    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            ComponentContext.Visible = value;
        }
    }

    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
