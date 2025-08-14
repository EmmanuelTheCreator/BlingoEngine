using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.FilmLoops;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Texts;
using LingoEngine.SDL2;
using LingoEngine.SDL2.Core;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.SDL2.Shapes;
using LingoEngine.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.SDL2.Sprites;

public class SdlSprite : ILingoFrameworkSprite, ILingoSDLComponent, IDisposable
{
    private readonly Action<SdlSprite> _show;
    private readonly Action<SdlSprite> _hide;
    private readonly Action<SdlSprite> _remove;
    private readonly LingoSprite2D _lingoSprite;
    public LingoSDLComponentContext ComponentContext { get; }
    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    private nint _texture = nint.Zero;
    private bool _textureOwned;

    private readonly SdlFactory _factory;

    public SdlSprite(LingoSprite2D sprite, SdlFactory factory, Action<SdlSprite> show, Action<SdlSprite> hide, Action<SdlSprite> remove)
    {
        _lingoSprite = sprite;
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

    private bool _visibility;
    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            ComponentContext.Visible = value;
        }
    }

    private float _blend = 1f;
    public float Blend
    {
        get => _blend;
        set
        {
            _blend = value;
            ApplyBlend();
        }
    }

    private float _x;
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            UpdateContextPosition();
        }
    }

    private float _y;
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            UpdateContextPosition();
        }
    }

    private float _width;
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

    private float _height;
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
    public string Name { get; set; } = string.Empty;
    public LingoPoint RegPoint { get; set; }
    public float DesiredHeight { get; set; }
    public float DesiredWidth { get; set; }
    private int _zIndex;
    public int ZIndex
    {
        get => _zIndex;
        set { _zIndex = value; }
    }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH
    {
        get => ComponentContext.FlipH;
        set => ComponentContext.FlipH = value;
    }
    public bool FlipV
    {
        get => ComponentContext.FlipV;
        set => ComponentContext.FlipV = value;
    }
    private bool _directToStage;
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

    private int _ink;
    private SDL.SDL_BlendMode _blendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
    public int Ink
    {
        get => _ink;
        set
        {
            _ink = value;
            ApplyInk();
            ComponentContext.QueueRedraw(this);
        }
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
    }

    public void Hide()
    {
        Visibility = false;
        _hide(this);
    }
    public void SetPosition(LingoPoint point) { X = point.X; Y = point.Y; }

    public void MemberChanged()
    {
        if (_lingoSprite.Member is { } member)
        {
            Width = member.Width;
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

    public LingoSDLRenderResult Render(LingoSDLRenderContext context)
    {
        Update();
        ComponentContext.Renderer = context.Renderer;
        if (_texture == nint.Zero)
        {
            return nint.Zero;
        }
        var offset = new LingoPoint();
        if (_lingoSprite.Member is { } member)
        {
            var baseOffset = member.CenterOffsetFromRegPoint();
            float scaleX = 1f;
            float scaleY = 1f;
            if (member.Width != 0 && member.Height != 0)
            {
                scaleX = Width / member.Width;
                scaleY = Height / member.Height;
            }
            offset = new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);

            if (_lingoSprite.Member is LingoFilmLoopMember flm)
            {
                var fl = flm.Framework<SdlMemberFilmLoop>();
                offset = new LingoPoint(offset.X - fl.Offset.X * scaleX, offset.Y - fl.Offset.Y * scaleY);
            }
        }

        ComponentContext.OffsetX = -offset.X;
        ComponentContext.OffsetY = -offset.Y;
        UpdateContextPosition();
        return _texture;
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        //if (_textureOwned && _texture != nint.Zero) // we may not dispose a texture because it can be used by other sprites.
        //    SDL.SDL_DestroyTexture(_texture);
        _texture = nint.Zero;
        _textureOwned = false;
        
        switch (_lingoSprite.Member)
        {
            case LingoMemberBitmap pic:
                pic.Preload();
                var p = pic.Framework<SdlMemberBitmap>();
                if (pic.TextureLingo is SdlTexture2D tex2D && tex2D.Texture!= nint.Zero)
                {
                    var texInk = p.GetTextureForInk(_lingoSprite.InkType, _lingoSprite.BackColor, ComponentContext.Renderer) as SdlTexture2D;
                    if (texInk != null && texInk.Texture != nint.Zero)
                        SetTextureOwned(texInk);
                    else
                    {
                        //var textureShape = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, p.Surface);
                        //SetTextureOwned(new SdlTexture2D(textureShape,p.Width, p.Height));
                        SetTextureOwned(tex2D);
                    }
                }
                break;
            case LingoFilmLoopMember flm:
                var fl = flm.Framework<SdlMemberFilmLoop>();
                if (fl.TextureLingo is SdlTexture2D tex && tex.Texture != nint.Zero)
                    SetTextureOwned(tex);
                break;
            case ILingoMemberTextBase text:
                text.FrameworkObj.Preload();
                var textureT = text.RenderToTexture(_lingoSprite.InkType, _lingoSprite.BackColor);
                if (textureT != null && textureT is SdlTexture2D sdlTexture)
                    SetTextureOwned(sdlTexture);
                break;
            case LingoMemberShape shape:
                shape.FrameworkObj.Preload();
                var textureS = shape.RenderToTexture(_lingoSprite.InkType, _lingoSprite.BackColor);
                if (textureS != null && textureS is SdlTexture2D sdlTexture2)
                    SetTextureOwned(sdlTexture2);
                //if (sh.Surface != nint.Zero)
                //{
                //    var textureShape = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, sh.Surface);
                //    if (_texture != nint.Zero)
                //    {
                //        _textureOwned = true;
                //        Width = sh.Width;
                //        Height = sh.Height;
                //    }
                //}
                break;
        }
        ApplyInk();
    }

    private void SetTextureOwned(SdlTexture2D tex)
    {
        _texture = tex.Texture;
        if (_texture == nint.Zero)
            return;
        _textureOwned = true;
        if (Width == 0 || Height == 0)
        {
            Width = tex.Width;
            Height = tex.Height;
        }
    }

    public void UpdateTexture(ILingoTexture2D texture)
    {
        var tex = (SdlTexture2D)texture;
        _texture = tex.Texture;
        if (!_textureOwned && (Width == 0 || Height == 0))
        {
            Width = tex.Width;
            Height = tex.Height;
        }
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
        ComponentContext.Blend = _directToStage ? 1f : _blend / 100;
    }

    private void UpdateContextPosition()
    {
        int x = (int)(_x - ComponentContext.TargetWidth / 2f);
        int y = (int)(_y - ComponentContext.TargetHeight / 2f);
        ComponentContext.X = x;
        ComponentContext.Y = y;
    }

    public void Resize(float w, float h) { Width = w; Height = h; }

    public void ApplyMemberChangesOnStepFrame()
    {

    }


}
