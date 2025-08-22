using System;
using AbstUI.LUnity.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Unity.Bitmaps;
using UnityEngine;

namespace LingoEngine.Unity.Sprites;

/// <summary>
/// Unity sprite wrapper used by the engine.
/// Mirrors the behaviour of the Godot implementation.
/// </summary>
public class LingoUnitySprite2D : ILingoFrameworkSprite, IDisposable
{
    // Fields
    private readonly GameObject _go;
    private readonly SpriteRenderer _renderer;
    private readonly Action<LingoUnitySprite2D> _show;
    private readonly Action<LingoUnitySprite2D> _hide;
    private readonly Action<LingoUnitySprite2D> _remove;
    private readonly LingoSprite2D _lingoSprite;

    private UnityTexture2D? _texture;
    private bool _visible;
    private float _blend = 100f;
    private float _x;
    private float _y;
    private string _name = string.Empty;
    private APoint _regPoint;
    private float _desiredWidth;
    private float _desiredHeight;
    private int _zIndex;
    private float _rotation;
    private bool _flipH;
    private bool _flipV;
    private bool _directToStage;
    private int _ink;

    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    // Properties
    public bool Visibility
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            _go.SetActive(value);
            if (value) _show(this); else _hide(this);
        }
    }

    public float Blend
    {
        get => _blend;
        set { _blend = value; ApplyBlend(); }
    }

    public float X { get => _x; set { _x = value; IsDirty = true; } }
    public float Y { get => _y; set { _y = value; IsDirty = true; } }

    public float Width { get; private set; }
    public float Height { get; private set; }

    public string Name { get => _name; set { _name = value; _go.name = value; } }
    public APoint RegPoint { get => _regPoint; set { _regPoint = value; IsDirty = true; } }
    public float DesiredHeight { get => _desiredHeight; set { _desiredHeight = value; IsDirty = true; } }
    public float DesiredWidth { get => _desiredWidth; set { _desiredWidth = value; IsDirty = true; } }

    public int ZIndex
    {
        get => _zIndex;
        set { _zIndex = value; ApplyZIndex(); }
    }

    public float Rotation { get => _rotation; set { _rotation = value; IsDirty = true; } }
    public float Skew { get; set; }

    public bool FlipH { get => _flipH; set { _flipH = value; IsDirty = true; } }
    public bool FlipV { get => _flipV; set { _flipV = value; IsDirty = true; } }

    public bool DirectToStage
    {
        get => _directToStage;
        set { _directToStage = value; ApplyZIndex(); ApplyBlend(); }
    }

    public int Ink { get => _ink; set { _ink = value; } }

    // Constructor
    public LingoUnitySprite2D(LingoSprite2D sprite, Transform parent,
        Action<LingoUnitySprite2D> show, Action<LingoUnitySprite2D> hide, Action<LingoUnitySprite2D> remove)
    {
        _show = show;
        _hide = hide;
        _remove = remove;
        _lingoSprite = sprite;

        _go = new GameObject(sprite.Name);
        _go.transform.parent = parent;
        _renderer = _go.AddComponent<SpriteRenderer>();

        sprite.Init(this);
        _name = sprite.Name;
        _zIndex = sprite.SpriteNum;
        _directToStage = sprite.DirectToStage;
        _ink = sprite.Ink;

        ApplyZIndex();
        ApplyBlend();
    }

    // Methods
    private void ApplyZIndex()
    {
        _renderer.sortingOrder = _directToStage ? 100000 + _zIndex : _zIndex;
    }

    private void ApplyBlend()
    {
        var c = _renderer.color;
        float alpha = Mathf.Clamp(_blend / 100f, 0f, 1f);
        c.a = _directToStage ? 1f : alpha;
        _renderer.color = c;
    }

    public void Resize(float w, float h)
    {
        DesiredWidth = w;
        DesiredHeight = h;
        IsDirty = true;
    }

    public void MemberChanged()
    {
        if (_lingoSprite.Member is { } member)
        {
            if (Width == 0 || Height == 0)
            {
                Width = member.Width;
                Height = member.Height;
            }
        }
        IsDirtyMember = true;
    }

    internal void Update()
    {
        if (IsDirtyMember)
            UpdateMember();

        if (!IsDirty) return;
        IsDirty = false;

        var pos = new Vector3(_x - _regPoint.X, _y - _regPoint.Y, 0f);
        _go.transform.localPosition = pos;
        _go.transform.localEulerAngles = new Vector3(0, 0, _rotation);

        if (_texture?.Texture is Texture2D tex)
        {
            float w = _desiredWidth == 0 ? tex.width : _desiredWidth;
            float h = _desiredHeight == 0 ? tex.height : _desiredHeight;
            float sx = w / tex.width;
            float sy = h / tex.height;
            if (_flipH) sx *= -1f;
            if (_flipV) sy *= -1f;
            _go.transform.localScale = new Vector3(sx, sy, 1f);
        }
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        switch (_lingoSprite.Member)
        {
            case LingoMemberBitmap bmp:
                var unityBmp = bmp.Framework<UnityMemberBitmap>();
                unityBmp.Preload();
                if (unityBmp.TextureLingo is UnityTexture2D tex)
                    SetTexture(tex);
                break;
                // Other member types (text, fields, shapes, film loops) not yet supported.
        }
        Name = _lingoSprite.GetFullName();
    }

    public void RemoveMe()
    {
        _remove(this);
        Dispose();
    }

    public void Show() => Visibility = true;
    public void Hide() => Visibility = false;

    public void SetPosition(APoint point)
    {
        X = point.X;
        Y = point.Y;
    }

    public void ApplyMemberChangesOnStepFrame() { }

    public void SetTexture(IAbstTexture2D texture)
    {
        if (texture is UnityTexture2D ut)
        {
            var tex = ut.Texture;
            if (tex == null)
                return;
            _renderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            Width = tex.width;
            Height = tex.height;
            _texture = ut;
            _lingoSprite.FWTextureHasChanged(ut);
            IsDirtyMember = true;
        }
    }

    public void Dispose()
    {
        GameObject.Destroy(_go);
    }
}
