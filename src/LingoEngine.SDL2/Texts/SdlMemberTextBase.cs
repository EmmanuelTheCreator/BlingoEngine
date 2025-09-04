using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Components.Texts;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Texts;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.SDL2.Texts;

public abstract class SdlMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    protected string _text = string.Empty;
    private readonly ISdlRootComponentContext _sdlRootContext;
    protected SdlFontManager _fontManager;
    private nint _surface;
    //private SdlTexture2D? _textureSDL;
    private ISdlFontLoadedByUser? _font;
    private int _fontSize = 12;
    private string _fontName = string.Empty;
    private int _measuredWidth;
    private string? _renderedText;
    private int _lastWidth;
    private int _lastHeight;
    private int _textWidth;
    private int _textHeight;
    private int _width;
    private LingoTextStyle _fontStyle;
    private bool _wordWrap;
    private AColor _textColor = AColor.FromRGB(150, 150, 150);
    private AbstTextAlignment _alignment;
    private int _height;

    public SdlTexture2D? TextureSDL => (SdlTexture2D?)_lingoMemberText.GetTexture();
    public IAbstTexture2D? TextureLingo => _lingoMemberText.GetTexture();


    public string Text
    {
        get => _text;
        set
        {
            var cleaned = AbstSdlLabel.CleanText(value);
            if (_text == cleaned) return;
            _text = cleaned;
            RequireRedraw();
            //if (_lingoMemberText is ILingoMemberTextBaseInteral internalText)
            //    internalText.FrameworkUpdateText(_text);
        }
    } // we need to trim the last newline to avoid text centered not rendering correctlycentered not rendering correctly
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap == value) return;
            _wordWrap = value;
            RequireRedraw();
        }
    }
    public int ScrollTop { get; set; }
    public string FontName
    {
        get => _fontName;
        set
        {
            if (_fontName == value) return;
            _fontName = value;// "Tahoma"; //value;
            _font = null;
            RequireRedraw();
        }
    }
    public int FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize == value) return;
            if (_fontSize <= 0) return;
            _fontSize = value;
            _font = null;
            RequireRedraw();
        }
    }
    public LingoTextStyle FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle == value) return;
            _fontStyle = value;
            RequireRedraw();
        }
    }
    public AColor TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor.Equals(value)) return;
            _textColor = value;
            RequireRedraw();
        }
    }
    public AbstTextAlignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment == value) return;
            _alignment = value;
            RequireRedraw();
        }
    }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }
    public int Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            RequireRedraw();
        }
    }
    public int Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            RequireRedraw();
        }
    }
    public IAbstFontManager FontManager => _fontManager;

    protected SdlMemberTextBase(IAbstFontManager fontManager, ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
        _fontManager = (SdlFontManager)fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
        _lingoMemberText.InitDefaults();
    }
    public void Dispose()
    {
        if (_surface != nint.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = nint.Zero;
        }
        //if (_font != null)
        //    _font.Release();
        //_font = null;
    }

    private void RequireRedraw()
    {
        _lingoMemberText.RequireRedraw();
    }
    public void Copy(string text) => SdlClipboard.SetText(text);
    public string PasteClipboard() => SdlClipboard.GetText();
    public string ReadText() => File.Exists(_lingoMemberText.FileName) ? File.ReadAllText(_lingoMemberText.FileName) : string.Empty;
    public void CopyToClipboard() => SdlClipboard.SetText(Text);
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() => _lingoMemberText.Text = SdlClipboard.GetText();
    public void Preload() { IsLoaded = true; }
    public void Unload()
    {
        IsLoaded = false;
        //if (_surface != nint.Zero)
        //{
        //    SDL.SDL_FreeSurface(_surface);
        //    _surface = nint.Zero;
        //}
        //if (_font != null)
        //    _font.Release();
        //_font = null;
    }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        //#if DEBUG
        //        if (_lingoMemberText.NumberInCast == 44)
        //        {

        //        }
        //#endif
        //        PreloadFont();

        //        if (_font == null || string.IsNullOrWhiteSpace(_text))
        //        {
        //            // empty texture
        //            // var fontSize = FontSize;
        //            // if (fontSize < 5)
        //            //     fontSize = 10;
        //            //_surface = AbstSdlLabel.CreateEmptySurface(fontSize, fontSize);
        //            // return new SdlTexture2D(_surface, fontSize, fontSize); 
        //            return null;
        //        }
        //        var font = _font.FontHandle;
        //        if (font == nint.Zero) return null;
        //        var text =  Text ?? string.Empty;
        //        // decide final box
        //        if (_renderedText != text)
        //            MeasureText(text);
        //        float boxW = Width > 0 ? Width : _textWidth;
        //        float boxH = Height > 0 ? Height : _textHeight;
        //        var (tex, textW, textH) = AbstSdlLabel.CreateTextTextureBox(_sdlRootContext.Renderer, font, text, (int)boxW, (int)boxH, Alignment, TextColor.ToSDLColor());
        //        _surface = tex;
        //        SDL.SDL_SetTextureBlendMode(_surface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        //        // update sizes
        //        if (Width <= 0) Width = (int)boxW;
        //        if (Height <= 0) Height = (int)boxH;

        //        _measuredWidth = textW;
        //        _renderedText = Text;
        //        _lastWidth = Width; 
        //        _lastHeight = Height;
        //        var offstX = Alignment == AbstTextAlignment.Right
        //            ? 0 : Alignment == AbstTextAlignment.Center ? 0 : -Width / 2;
        //        _lingoMemberText.RegPoint = new APoint(offstX, -Height/2);

        // make a copy
        //var copyTextureSDL = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), (int)boxW, (int)boxH);
        //return new SdlTexture2D(_surface, (int)boxW, (int)boxH,text);
        return _lingoMemberText.GetTexture();
    }

    private void PreloadFont()
    {
        if (_font == null)
            _font = _fontManager.GetTyped(this, FontName, FontSize);
    }



    /// <summary>Returns the bounding box (w,h) for multiline text (handles \n). No wrapping.</summary>
    private void MeasureText(string text)
    {
        var (maxW, totalH) = AbstSdlLabel.MeasureSDLText(_font!.FontHandle, text);
        _textWidth = maxW;
        _textHeight = totalH;
    }
    public bool IsPixelTransparent(int x, int y) => false;
}
