using System;
using AbstUI.LUnity.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Shapes;
using LingoEngine.FilmLoops;
using LingoEngine.Medias;
using LingoEngine.Unity.Bitmaps;
using LingoEngine.Unity.Texts;
using LingoEngine.Unity.Shapes;
using LingoEngine.Unity.FilmLoops;
using LingoEngine.Unity.Medias;
using UnityEngine;

namespace LingoEngine.Unity.Sprites;

/// <summary>
/// Unity sprite wrapper used by the engine.
/// Mirrors the behaviour of the Godot implementation.
/// </summary>
public class LingoUnitySprite2D : ILingoFrameworkSprite, ILingoFrameworkSpriteVideo, IDisposable
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
    private LingoFilmLoopPlayer? _filmLoopPlayer;
    private VideoPlayer? _videoPlayer;

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
                if (unityBmp.TextureLingo is UnityTexture2D texBmp)
                    SetTexture(texBmp);
                break;
            case LingoFilmLoopMember flm:
                var unityFl = flm.Framework<UnityFilmLoopMember>();
                unityFl.Preload();
                _filmLoopPlayer = _lingoSprite.GetFilmLoopPlayer();
                var size = _filmLoopPlayer?.GetBoundingBox() ?? flm.GetBoundingBox();
                _desiredWidth = size.Width;
                _desiredHeight = size.Height;
                Width = size.Width;
                Height = size.Height;
                if (_filmLoopPlayer?.Texture is UnityTexture2D texFl)
                    SetTexture(texFl);
                else if (unityFl.TextureLingo is UnityTexture2D texFl2)
                    SetTexture(texFl2);
                break;
            case LingoMemberText text:
                var unityText = text.Framework<UnityMemberText>();
                unityText.Preload();
                if (unityText.RenderToTexture(_lingoSprite.InkType, _lingoSprite.BackColor) is UnityTexture2D texText)
                    SetTexture(texText);
                break;
            case LingoMemberField field:
                var unityField = field.Framework<UnityMemberField>();
                unityField.Preload();
                if (unityField.RenderToTexture(_lingoSprite.InkType, _lingoSprite.BackColor) is UnityTexture2D texField)
                    SetTexture(texField);
                break;
            case LingoMemberShape shape:
                var unityShape = shape.Framework<UnityMemberShape>();
                unityShape.Preload();
                if (unityShape.TextureLingo is UnityTexture2D texShape)
                    SetTexture(texShape);
                break;
            case LingoMemberMedia media:
                var unityMedia = media.Framework<LingoUnityMemberMedia>();
                unityMedia.Preload();
                UpdateMemberVideo(unityMedia);
                break;
        }
        if (_lingoSprite.Member is { } member)
        {
            if (Width == 0 || Height == 0)
            {
                Width = member.Width;
                Height = member.Height;
            }
        }
        Name = _lingoSprite.GetFullName();
    }

    private void UpdateMemberVideo(LingoUnityMemberMedia media)
    {
        if (_videoPlayer != null)
        {
            UnityEngine.Object.Destroy(_videoPlayer.gameObject);
            _videoPlayer = null;
        }

        var go = new GameObject("VideoPlayer");
        go.transform.parent = _go.transform;
        var player = go.AddComponent<VideoPlayer>();
        if (media.Url != null)
            player.url = media.Url;
        player.playOnAwake = false;
        _videoPlayer = player;
    }

    /// <inheritdoc/>
    public void Play() => _videoPlayer?.Play();

    /// <inheritdoc/>
    public void Pause() => _videoPlayer?.Pause();

    /// <inheritdoc/>
    public void Stop()
    {
        if (_videoPlayer == null) return;
        _videoPlayer.Stop();
        _videoPlayer.time = 0;
    }

    /// <inheritdoc/>
    public void Seek(int milliseconds)
    {
        if (_videoPlayer == null) return;
        _videoPlayer.time = milliseconds / 1000.0;
    }

    /// <inheritdoc/>
    public int Duration => _videoPlayer?.clip != null ? (int)(_videoPlayer.clip.length * 1000) : 0;

    /// <inheritdoc/>
    public int CurrentTime
    {
        get => (int)(_videoPlayer?.time * 1000 ?? 0);
        set
        {
            if (_videoPlayer != null)
                _videoPlayer.time = value / 1000.0;
        }
    }

    /// <inheritdoc/>
    public LingoMediaStatus MediaStatus => _videoPlayer switch
    {
        null => LingoMediaStatus.Closed,
        _ when _videoPlayer.isPlaying => LingoMediaStatus.Playing,
        _ => LingoMediaStatus.Paused
    };

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
        if (_videoPlayer != null)
            UnityEngine.Object.Destroy(_videoPlayer.gameObject);
        GameObject.Destroy(_go);
    }
}
