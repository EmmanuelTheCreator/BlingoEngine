using System;

namespace AbstUI.ImGui.Components;

/// <summary>
/// Base class for SDL GFX components providing common geometry and visibility
/// handling that synchronizes with the <see cref="AbstImGuiComponentContext"/>.
/// </summary>
public abstract class AbstImGuiComponent : IAbstImGuiComponent, IDisposable
{
    protected AbstImGuiComponent(AbstImGuiComponentFactory factory)
    {
        ComponentContext = factory.CreateContext(this);
        ComponentContext.Visible = _visibility;
    }

    public AbstImGuiComponentContext ComponentContext { get; }

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
    public abstract AbstImGuiRenderResult Render(AbstImGuiRenderContext context);

    /// <inheritdoc />
    public virtual void Dispose() => ComponentContext.Dispose();
}

