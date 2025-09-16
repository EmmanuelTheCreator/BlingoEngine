using System;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Sprites;
using LingoEngine.Medias;
using LingoEngine.Members;
using LingoEngine.Blazor.Medias;
using Microsoft.JSInterop;
using AbstUI.Blazor;
using LingoEngine.Blazor.Util;
using AbstUI.Components;

namespace LingoEngine.Blazor.Sprites;

/// <summary>
/// Simple sprite implementation for the Blazor backend. It stores sprite
/// properties and notifies the movie about visibility changes but does not
/// perform actual rendering.
/// </summary>
public class LingoBlazorSprite2D : ILingoFrameworkSprite, ILingoFrameworkSpriteVideo, IDisposable
{
    private readonly Action<LingoBlazorSprite2D> _show;
    private readonly Action<LingoBlazorSprite2D> _hide;
    private readonly Action<LingoBlazorSprite2D> _remove;
    private readonly LingoSprite2D _lingoSprite;
    private readonly AbstUIScriptResolver _scripts;
    private IAbstTexture2D? _texture;
    private string? _videoUrl;
    private IJSObjectReference? _video;
    private DotNetObjectReference<object>? _videoRef;
    private int _duration;
    private int _currentTime;
    private LingoMediaStatus _mediaStatus;

    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;
    private AMargin _margin = AMargin.Zero;

    public event Action? Changed;

    private void MakeDirty()
    {
        IsDirty = true;
        Changed?.Invoke();
    }

    public LingoBlazorSprite2D(LingoSprite2D sprite,
        Action<LingoBlazorSprite2D> show,
        Action<LingoBlazorSprite2D> hide,
        Action<LingoBlazorSprite2D> remove,
        AbstUIScriptResolver scripts)
    {
        _show = show;
        _hide = hide;
        _remove = remove;
        _scripts = scripts;
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
            MakeDirty();
        }
    }
    private bool _visible;

    private float _blend = 1f;
    public float Blend { get => _blend; set { _blend = value; MakeDirty(); } }
    private float _x;
    public float X { get => _x; set { _x = value; MakeDirty(); } }
    private float _y;
    public float Y { get => _y; set { _y = value; MakeDirty(); } }
    public float Width { get; private set; }
    public float Height { get; private set; }
    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; MakeDirty(); } }
    private APoint _regPoint;
    public APoint RegPoint { get => _regPoint; set { _regPoint = value; MakeDirty(); } }
    private float _desiredHeight;
    public float DesiredHeight { get => _desiredHeight; set { _desiredHeight = value; MakeDirty(); } }
    private float _desiredWidth;
    public float DesiredWidth { get => _desiredWidth; set { _desiredWidth = value; MakeDirty(); } }
    private int _zIndex;
    public int ZIndex { get => _zIndex; set { _zIndex = value; MakeDirty(); } }
    private float _rotation;
    public float Rotation { get => _rotation; set { _rotation = value; MakeDirty(); } }
    private float _skew;
    public float Skew { get => _skew; set { _skew = value; MakeDirty(); } }
    private bool _flipH;
    public bool FlipH { get => _flipH; set { _flipH = value; MakeDirty(); } }
    private bool _flipV;
    public bool FlipV { get => _flipV; set { _flipV = value; MakeDirty(); } }
    private bool _directToStage;
    public bool DirectToStage { get => _directToStage; set { _directToStage = value; MakeDirty(); } }
    private int _ink;
    public int Ink { get => _ink; set { _ink = value; MakeDirty(); } }

    public AMargin Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            MakeDirty();
        }
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
            if (member is LingoMemberMedia media)
            {
                var blazorMedia = media.Framework<LingoBlazorMemberMedia>();
                blazorMedia.Preload();
                _videoUrl = blazorMedia.Url;
                if (_videoUrl != null)
                {
                    _videoRef?.Dispose();
                    _videoRef = DotNetObjectReference.Create<object>(this);
                    var id = ElementIdGenerator.Create(_lingoSprite.Member?.Name ?? "video");
                    _video = _scripts.MediaCreateVideo(id, _videoUrl, _videoRef).GetAwaiter().GetResult();
                    _duration = (int)_scripts.MediaGetDuration(_video).GetAwaiter().GetResult();
                }
                _mediaStatus = blazorMedia.MediaStatus;
            }
            IsDirtyMember = true;
            MakeDirty();
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
        MakeDirty();
    }

    /// <inheritdoc/>
    public void Play()
    {
        if (_video != null)
            _scripts.MediaPlayVideo(_video).GetAwaiter().GetResult();
        _mediaStatus = LingoMediaStatus.Playing;
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (_video != null)
            _scripts.MediaStopVideo(_video).GetAwaiter().GetResult();
        _mediaStatus = LingoMediaStatus.Closed;
        _currentTime = 0;
    }

    /// <inheritdoc/>
    public void Pause()
    {
        if (_video != null)
            _scripts.MediaPauseVideo(_video).GetAwaiter().GetResult();
        _mediaStatus = LingoMediaStatus.Paused;
    }

    /// <inheritdoc/>
    public void Seek(int milliseconds)
    {
        if (_video != null)
            _scripts.MediaSeekVideo(_video, milliseconds / 1000.0).GetAwaiter().GetResult();
        _currentTime = milliseconds;
    }

    /// <inheritdoc/>
    public int Duration => _duration;

    /// <inheritdoc/>
    public int CurrentTime
    {
        get => _currentTime;
        set
        {
            if (_video != null)
                _scripts.MediaSeekVideo(_video, value / 1000.0).GetAwaiter().GetResult();
            _currentTime = value;
        }
    }

    object IAbstFrameworkNode.FrameworkNode => this;

    float IAbstFrameworkNode.Width
    {
        get => Width;
        set
        {
            Width = value;
            if (_desiredWidth == 0)
                _desiredWidth = value;
            MakeDirty();
        }
    }

    float IAbstFrameworkNode.Height
    {
        get => Height;
        set
        {
            Height = value;
            if (_desiredHeight == 0)
                _desiredHeight = value;
            MakeDirty();
        }
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => Margin;
        set => Margin = value;
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => ZIndex;
        set => ZIndex = value;
    }

    /// <inheritdoc/>
    public LingoMediaStatus MediaStatus => _mediaStatus;

    internal void Update()
    {
        IsDirty = false;
        IsDirtyMember = false;
    }

    internal IAbstTexture2D? Texture => _texture;
    internal IJSObjectReference? Video => _video;

    internal void UpdateCurrentTime(double ms) => _currentTime = (int)ms;

    [JSInvokable]
    public void OnVideoEnded() => _lingoSprite.Stop();

    public void Dispose()
    {
        _video?.DisposeAsync().GetAwaiter().GetResult();
        _videoRef?.Dispose();
        _remove(this);
    }
}
