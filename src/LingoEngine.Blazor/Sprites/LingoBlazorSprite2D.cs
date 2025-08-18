using System;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Sprites;

namespace LingoEngine.Blazor.Sprites;

/// <summary>
/// Simple sprite implementation for the Blazor backend. It stores sprite
/// properties and notifies the movie about visibility changes but does not
/// perform actual rendering.
/// </summary>
public class LingoBlazorSprite2D : ILingoFrameworkSprite, IDisposable
{
    private readonly Action<LingoBlazorSprite2D> _show;
    private readonly Action<LingoBlazorSprite2D> _hide;
    private readonly Action<LingoBlazorSprite2D> _remove;
    private readonly LingoSprite2D _lingoSprite;
    private IAbstTexture2D? _texture;

    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    public LingoBlazorSprite2D(LingoSprite2D sprite,
        Action<LingoBlazorSprite2D> show,
        Action<LingoBlazorSprite2D> hide,
        Action<LingoBlazorSprite2D> remove)
    {
        _show = show;
        _hide = hide;
        _remove = remove;
        _lingoSprite = sprite;
        sprite.Init(this);
        ZIndex = sprite.SpriteNum;
        DirectToStage = sprite.DirectToStage;
        Ink = sprite.Ink;
    }

    public bool Visibility
    {
        get => _visible;
        set
        {
            _visible = value;
            if (value) _show(this); else _hide(this);
            IsDirty = true;
        }
    }
    private bool _visible;

    private float _blend = 1f;
    public float Blend { get => _blend; set { _blend = value; IsDirty = true; } }
    private float _x;
    public float X { get => _x; set { _x = value; IsDirty = true; } }
    private float _y;
    public float Y { get => _y; set { _y = value; IsDirty = true; } }
    public float Width { get; private set; }
    public float Height { get; private set; }
    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; IsDirty = true; } }
    private APoint _regPoint;
    public APoint RegPoint { get => _regPoint; set { _regPoint = value; IsDirty = true; } }
    private float _desiredHeight;
    public float DesiredHeight { get => _desiredHeight; set { _desiredHeight = value; IsDirty = true; } }
    private float _desiredWidth;
    public float DesiredWidth { get => _desiredWidth; set { _desiredWidth = value; IsDirty = true; } }
    private int _zIndex;
    public int ZIndex { get => _zIndex; set { _zIndex = value; IsDirty = true; } }
    private float _rotation;
    public float Rotation { get => _rotation; set { _rotation = value; IsDirty = true; } }
    private float _skew;
    public float Skew { get => _skew; set { _skew = value; IsDirty = true; } }
    private bool _flipH;
    public bool FlipH { get => _flipH; set { _flipH = value; IsDirty = true; } }
    private bool _flipV;
    public bool FlipV { get => _flipV; set { _flipV = value; IsDirty = true; } }
    private bool _directToStage;
    public bool DirectToStage { get => _directToStage; set { _directToStage = value; IsDirty = true; } }
    private int _ink;
    public int Ink { get => _ink; set { _ink = value; IsDirty = true; } }

    public void MemberChanged()
    {
        if (_lingoSprite.Member is { } member)
        {
            if (Width == 0 || Height == 0)
            {
                Width = member.Width;
                Height = member.Height;
            }
            IsDirtyMember = true;
        }
    }

    public void RemoveMe() => _remove(this);
    public void Show() => Visibility = true;
    public void Hide() => Visibility = false;
    public void SetPosition(APoint point) { X = point.X; Y = point.Y; }
    public void ApplyMemberChangesOnStepFrame() { }

    public void SetTexture(IAbstTexture2D texture)
    {
        _texture = texture;
        if (Width == 0 || Height == 0)
        {
            Width = texture.Width;
            Height = texture.Height;
        }
        _lingoSprite.FWTextureHasChanged(texture);
        IsDirtyMember = true;
    }

    internal void Update()
    {
        IsDirty = false;
        IsDirtyMember = false;
    }

    internal IAbstTexture2D? Texture => _texture;

    public void Dispose() => _remove(this);
}
