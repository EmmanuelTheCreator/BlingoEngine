using AbstUI.SDL2.Core;
using System;

namespace AbstUI.SDL2.Components.Base;

/// <summary>
/// Base class for SDL GFX components providing common geometry and visibility
/// handling that synchronizes with the <see cref="AbstSDLComponentContext"/>.
/// </summary>
public abstract class AbstSdlComponent : IAbstSDLComponent, IDisposable
{
    protected AbstSdlComponentFactory Factory { get; }
    public AbstSDLComponentContext ComponentContext { get; }

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
    public virtual float Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > float.Epsilon)
            {
                _width = value;
                ComponentContext.TargetWidth = (int)value;
                ComponentContext.QueueRedraw(this);
            }
        }
    }

    private float _height;
    public virtual float Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) > float.Epsilon)
            {
                _height = value;
                ComponentContext.TargetHeight = (int)value;
                ComponentContext.QueueRedraw(this);
            }
        }
    }

    public virtual int ZIndex
    {
        get => ComponentContext.ZIndex;
        set => ComponentContext.SetZIndex(value);
    }

    private bool _visibility = true;
    public virtual bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            ComponentContext.Visible = value;
        }
    }

    public virtual string Name { get; set; } = string.Empty;


    protected AbstSdlComponent(AbstSdlComponentFactory factory, AbstSDLComponentContext? parent = null)
    {
        Factory = factory;
        ComponentContext = factory.CreateContext(this, parent);
        ComponentContext.Visible = _visibility;
    }
    /// <inheritdoc />
    public abstract AbstSDLRenderResult Render(AbstSDLRenderContext context);

    /// <inheritdoc />
    public virtual void Dispose() => ComponentContext.Dispose();
}

