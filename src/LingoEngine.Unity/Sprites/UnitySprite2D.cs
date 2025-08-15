using System;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Sprites;
using UnityEngine;

namespace LingoEngine.Unity.Sprites;

/// <summary>
/// Minimal Unity sprite wrapper used by the engine.
/// </summary>
public class UnitySprite2D : ILingoFrameworkSprite, IDisposable
{
    private readonly GameObject _go;
    private readonly SpriteRenderer _renderer;
    private readonly Action<UnitySprite2D> _show;
    private readonly Action<UnitySprite2D> _hide;
    private readonly LingoSprite2D _lingoSprite;
    private readonly Action<UnitySprite2D> _remove;
    private bool _dirty;

    public UnitySprite2D(LingoSprite2D sprite, Transform parent,
        Action<UnitySprite2D> show, Action<UnitySprite2D> hide, Action<UnitySprite2D> remove)
    {
        _show = show;
        _hide = hide;
        _lingoSprite = sprite;
        _remove = remove;
        _go = new GameObject(sprite.Name);
        _go.transform.parent = parent;
        _renderer = _go.AddComponent<SpriteRenderer>();
        sprite.Init(this);
        ZIndex = sprite.SpriteNum;
    }

    public bool Visibility
    {
        get => _go.activeSelf;
        set
        {
            _go.SetActive(value);
            if (value) _show(this); else _hide(this);
        }
    }

    public float Blend
    {
        get => _renderer.color.a;
        set { var c = _renderer.color; c.a = value; _renderer.color = c; }
    }

    public float X
    {
        get => _go.transform.localPosition.x;
        set { var p = _go.transform.localPosition; p.x = value; _go.transform.localPosition = p; }
    }

    public float Y
    {
        get => _go.transform.localPosition.y;
        set { var p = _go.transform.localPosition; p.y = value; _go.transform.localPosition = p; }
    }

    public float Width { get; private set; }
    public float Height { get; private set; }

    public string Name
    {
        get => _go.name;
        set => _go.name = value;
    }

    public APoint RegPoint { get; set; }
    public float DesiredHeight { get; set; }
    public float DesiredWidth { get; set; }

    public int ZIndex
    {
        get => _renderer.sortingOrder;
        set => _renderer.sortingOrder = value;
    }

    public float Rotation
    {
        get => _go.transform.localEulerAngles.z;
        set { var e = _go.transform.localEulerAngles; e.z = value; _go.transform.localEulerAngles = e; }
    }

    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public bool DirectToStage { get; set; }
    public int Ink { get; set; }

    internal void Update()
    {
        if (_dirty)
            _dirty = false;
    }

    public void Resize(float w, float h)
    {
        DesiredWidth = w;
        DesiredHeight = h;
        _go.transform.localScale = new Vector3(w, h, 1f);
    }

    public void MemberChanged()
    {
        if (_lingoSprite.Member is { } member)
        {
            Width = member.Width;
            Height = member.Height;
        }
        _dirty = true;
    }

    public void RemoveMe() => _remove(this);

    public void Show() => Visibility = true;

    public void Hide() => Visibility = false;

    public void SetPosition(APoint point)
    {
        X = point.X;
        Y = point.Y;
    }

    public void ApplyMemberChangesOnStepFrame()
    {
    }

    public void SetTexture(ILingoTexture2D texture)
    {
        if (texture is Unity.Bitmaps.UnityTexture2D ut)
        {
            var tex = ut.Texture;
            _renderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            Width = tex.width;
            Height = tex.height;
        }
    }

    public void Dispose()
    {
        _remove(this);
        GameObject.Destroy(_go);
    }
}

