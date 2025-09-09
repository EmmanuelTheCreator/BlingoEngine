using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.FilmLoops;
using LingoEngine.Medias;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Core;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Shapes;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using LingoEngine.SDL2.Bitmaps;
using LingoEngine.SDL2.FilmLoops;
using LingoEngine.SDL2.Medias;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;

namespace LingoEngine.SDL2.Sprites;

public class SdlSprite : ILingoFrameworkSprite, ILingoFrameworkSpriteVideo, IAbstSDLComponent, IDisposable
{
    private readonly Action<SdlSprite> _show;
    private readonly Action<SdlSprite> _hide;
    private readonly Action<SdlSprite> _remove;
    private readonly LingoSprite2D _lingoSprite2D;
    public AbstSDLComponentContext ComponentContext { get; }
    internal bool _somethingChanged;
    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    private nint _texture = nint.Zero;
    private nint _lastTexture = nint.Zero;
    private bool _textureOwned;
    private int _zIndex;
    private bool _directToStage;
    private int _ink;
    private SDL.SDL_BlendMode _blendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
    private float _rotation;
    private float _skew;
    private APoint _regPoint;
    private bool _visibility;
    private float _blend = 1f;
    private float _x;
    private float _y;
    private float _width;
    private float _height;

    private readonly LingoSdlFactory _factory;

    #region Properties
    public string Name { get; set; } = string.Empty;
    public float DesiredHeight { get; set; }
    public float DesiredWidth { get; set; }

    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            ComponentContext.Visible = value;
            _somethingChanged = true;
        }
    }
    public float Blend
    {
        get => _blend;
        set
        {
            _blend = value;
            _somethingChanged = true;
            ApplyBlend();
        }
    }
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            UpdateContextPosition();
        }
    }
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            UpdateContextPosition();
        }
    }
    public float Width
    {
        get => _width;
        private set
        {
            _width = value;
            ComponentContext.TargetWidth = (int)value;
            UpdateContextPosition();
        }
    }
    public float Height
    {
        get => _height;
        private set
        {
            _height = value;
            ComponentContext.TargetHeight = (int)value;
            UpdateContextPosition();
        }
    }
    public APoint RegPoint
    {
        get => _regPoint;
        set
        {
            _regPoint = value;
            _somethingChanged = true;
        }
    }
    public int ZIndex
    {
        get => _zIndex;
        set { _zIndex = value; _somethingChanged = true; }
    }
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _somethingChanged = true;
        }
    }
    public float Skew
    {
        get => _skew;
        set
        {
            _skew = value;
            _somethingChanged = true;
        }
    }
    public bool FlipH
    {
        get => ComponentContext.FlipH;
        set
        {
            ComponentContext.FlipH = value;
            _somethingChanged = true;
        }
    }
    public bool FlipV
    {
        get => ComponentContext.FlipV;
        set
        {
            ComponentContext.FlipV = value;
            _somethingChanged = true;
        }
    }

    public bool DirectToStage
    {
        get => _directToStage;
        set
        {
            _directToStage = value;
            ApplyBlend();
            ComponentContext.QueueRedraw(this);
        }
    }



    public int Ink
    {
        get => _ink;
        set
        {
            _ink = value;
            _somethingChanged = true;
            ApplyInk();
            ComponentContext.QueueRedraw(this);
        }
    }

    public int Duration
    {
        get
        {
            if (_lingoSprite2D.Member is LingoMemberMedia media)
            {
                return media.Framework<SdlMemberMedia>()?.Duration ?? 0;
            }
            return 0;
        }
    }

    public int CurrentTime
    {
        get
        {
            if (_lingoSprite2D.Member is LingoMemberMedia media)
            {
                return media.Framework<SdlMemberMedia>()?.CurrentTime ?? 0;
            }
            return 0;
        }
        set
        {
            if (_lingoSprite2D.Member is LingoMemberMedia media)
            {
                media.Framework<SdlMemberMedia>()?.Seek(value);
            }
        }
    }

    public LingoMediaStatus MediaStatus
    {
        get
        {
            if (_lingoSprite2D.Member is LingoMemberMedia media)
            {
                return media.Framework<SdlMemberMedia>()?.MediaStatus ?? LingoMediaStatus.Closed;
            }
            return LingoMediaStatus.Closed;
        }
    }

    #endregion

    public SdlSprite(LingoSprite2D sprite, LingoSdlFactory factory, Action<SdlSprite> show, Action<SdlSprite> hide, Action<SdlSprite> remove)
    {
        _lingoSprite2D = sprite;
        _factory = factory;
        ComponentContext = factory.CreateContext(this);
        _show = show;
        _hide = hide;
        _remove = remove;
        sprite.Init(this);
        _zIndex = sprite.SpriteNum;
        _directToStage = sprite.DirectToStage;
        _ink = sprite.Ink;
        ApplyBlend();
        ApplyInk();
    }
    public void Play()
    {
        if (_lingoSprite2D.Member is LingoMemberMedia media)
            media.Framework<SdlMemberMedia>()?.Play();
    }

    public void Pause()
    {
        if (_lingoSprite2D.Member is LingoMemberMedia media)
            media.Framework<SdlMemberMedia>()?.Pause();
    }

    public void Stop()
    {
        if (_lingoSprite2D.Member is LingoMemberMedia media)
            media.Framework<SdlMemberMedia>()?.Stop();
    }

    public void Seek(int milliseconds)
    {
        if (_lingoSprite2D.Member is LingoMemberMedia media)
            media.Framework<SdlMemberMedia>()?.Seek(milliseconds);
    }

    public void RemoveMe() { _remove(this); Dispose(); }
    public void Dispose()
    {
        ComponentContext.Dispose();
        if (_textureOwned && _texture != nint.Zero)
            SDL.SDL_DestroyTexture(_texture);
        _texture = nint.Zero;
        _textureOwned = false;
    }
    public void Show()
    {
        Visibility = true;
        _show(this);
        _somethingChanged = true;
    }

    public void Hide()
    {
        Visibility = false;
        _hide(this);
        _somethingChanged = true;
    }
    public void SetPosition(APoint point) { X = point.X; Y = point.Y; }

    public void MemberChanged()
    {
        if (_lingoSprite2D.Member is { } member)
        {
            if(Width ==0)
                Width = member.Width;
            if (Height == 0)
                Height = member.Height;
        }
        IsDirtyMember = true;
        ComponentContext.QueueRedraw(this);
    }

    internal void Update()
    {
        if (IsDirtyMember)
            UpdateMember();
        if (IsDirty)
        {
            if (DesiredWidth != 0) Width = DesiredWidth;
            if (DesiredHeight != 0) Height = DesiredHeight;
            IsDirty = false;
        }
    }

    public AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!IsDirty && !IsDirtyMember && !_somethingChanged)
        {
            return _lastTexture;
        }
        _somethingChanged = false;
        Update();
        ComponentContext.Renderer = context.Renderer;
        if (_texture == nint.Zero)
        {
            return nint.Zero;
        }
        var offset = new APoint();
        if (_lingoSprite2D.Member is { } member)
        {
            var baseOffset = member.CenterOffsetFromRegPoint();
            float scaleX = 1f;
            float scaleY = 1f;
            if (member.Width != 0 && member.Height != 0)
            {
                scaleX = Width / member.Width;
                scaleY = Height / member.Height;
            }
            offset = new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);

            if (_lingoSprite2D.Member is LingoFilmLoopMember flm)
            {
                var fl = flm.Framework<SdlMemberFilmLoop>();
                offset = new APoint(offset.X - fl.Offset.X * scaleX, offset.Y - fl.Offset.Y * scaleY);
            }
        }

        ComponentContext.OffsetX = -offset.X;
        ComponentContext.OffsetY = -offset.Y;
        UpdateContextPosition();
        _lastTexture = _texture;
        return _texture;
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        //if (_textureOwned && _texture != nint.Zero) // we may not dispose a texture because it can be used by other sprites.
        //    SDL.SDL_DestroyTexture(_texture);
        _texture = nint.Zero;
        _textureOwned = false;

        switch (_lingoSprite2D.Member)
        {
            case LingoMemberBitmap pic:
                pic.Preload();
                var p = pic.Framework<SdlMemberBitmap>();
                if (pic.TextureLingo is SdlTexture2D tex2D && tex2D.Handle != nint.Zero)
                {
                    var texInk = p.GetTextureForInk(_lingoSprite2D.InkType, _lingoSprite2D.BackColor, ComponentContext.Renderer) as SdlTexture2D;
                    if (texInk != null && texInk.Handle != nint.Zero)
                        TextureHasChanged(texInk);
                    else
                    {
                        //var textureShape = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, p.Surface);
                        //SetTextureOwned(new SdlTexture2D(textureShape,p.Width, p.Height));
                        TextureHasChanged(tex2D);
                    }
                }
                break;
            case LingoFilmLoopMember flm:
                var fl = flm.Framework<SdlMemberFilmLoop>();
                if (fl.TextureLingo is SdlTexture2D tex && tex.Handle != nint.Zero)
                    TextureHasChanged(tex);
                break;
            case ILingoMemberTextBase text:
                text.FrameworkObj.Preload();
                var textureT = text.RenderToTexture(_lingoSprite2D.InkType, _lingoSprite2D.BackColor);
                if (textureT != null && textureT is SdlTexture2D sdlTexture)
                    TextureHasChanged(sdlTexture);
                break;
            case LingoMemberShape shape:
                shape.FrameworkObj.Preload();
                var textureS = shape.RenderToTexture(_lingoSprite2D.InkType, _lingoSprite2D.BackColor);
                if (textureS != null && textureS is SdlTexture2D sdlTexture2)
                    TextureHasChanged(sdlTexture2);
                break;
        }
        ApplyInk();
    }

    private void TextureHasChanged(SdlTexture2D tex)
    {
        _texture = tex.Handle;
        _lingoSprite2D.FWTextureHasChanged(tex);
        if (_texture == nint.Zero)
            return;
        _textureOwned = true;
        if (Width == 0 || Height == 0)
        {
            Width = tex.Width;
            Height = tex.Height;
        }
    }

    public void SetTexture(IAbstTexture2D texture)
    {
        var tex = (SdlTexture2D)texture;
        TextureHasChanged(tex);
    }

    private static readonly SDL.SDL_BlendMode _subtractBlend = SDL.SDL_ComposeCustomBlendMode(
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_SUBTRACT,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_SUBTRACT);

    private static readonly SDL.SDL_BlendMode _lightestBlend = SDL.SDL_ComposeCustomBlendMode(
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_MAXIMUM,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_MAXIMUM);

    private static readonly SDL.SDL_BlendMode _darkestBlend = SDL.SDL_ComposeCustomBlendMode(
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_MINIMUM,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
        SDL.SDL_BlendOperation.SDL_BLENDOPERATION_MINIMUM);

    private void ApplyInk()
    {
        _blendMode = _ink switch
        {
            (int)LingoInkType.AddPin => SDL.SDL_BlendMode.SDL_BLENDMODE_ADD,
            (int)LingoInkType.Add => SDL.SDL_BlendMode.SDL_BLENDMODE_ADD,
            (int)LingoInkType.SubstractPin => _subtractBlend,
            (int)LingoInkType.Substract => _subtractBlend,
            (int)LingoInkType.Darken => SDL.SDL_BlendMode.SDL_BLENDMODE_MOD,
            (int)LingoInkType.Lighten => SDL.SDL_BlendMode.SDL_BLENDMODE_ADD,
            (int)LingoInkType.Lightest => _lightestBlend,
            (int)LingoInkType.Darkest => _darkestBlend,
            _ => SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND,
        };
        ComponentContext.BlendMode = _blendMode;
    }

    private void ApplyBlend()
    {
        //ComponentContext.Blend = _directToStage ? 1f : _blend;
        ComponentContext.Blend = _directToStage ? 100 : _blend;
        _somethingChanged = true;
    }

    private void UpdateContextPosition()
    {
        int x = (int)(_x - ComponentContext.TargetWidth / 2f);
        int y = (int)(_y - ComponentContext.TargetHeight / 2f);
        ComponentContext.X = x;
        ComponentContext.Y = y;
        _somethingChanged = true;
    }

    public void Resize(float w, float h) { Width = w; Height = h; }

    public void ApplyMemberChangesOnStepFrame()
    {
        IsDirtyMember = true;
        ComponentContext.QueueRedraw(this);
    }


}
