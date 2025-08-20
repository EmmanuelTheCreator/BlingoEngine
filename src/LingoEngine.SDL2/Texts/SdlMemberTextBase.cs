using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Components.Texts;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Texts;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using System.Runtime.InteropServices;

namespace LingoEngine.SDL2.Texts;

public abstract class SdlMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    protected string _text = string.Empty;
    private readonly ISdlRootComponentContext _sdlRootContext;
    protected SdlFontManager _fontManager;
    private nint _surface;
    public SdlTexture2D? _textureSDL;
    private ISdlFontLoadedByUser? _font;
    private int _fontSize;
    private string _fontName = string.Empty;
    private int _measuredWidth;
    private string? _renderedText;
    private int _lastWidth;
    private int _lastHeight;
    private int _textWidth;
    private int _textHeight;
    private int _width;

    public SdlTexture2D? TextureSDL => _textureSDL;
    public IAbstTexture2D? TextureLingo => _textureSDL;


    public string Text { get => _text; set => _text = AbstSdlLabel.CleanText(value); } // we need to trim the last newline to avoid text centered not rendering correctlycentered not rendering correctly
    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }
    public string FontName
    {
        get => _fontName;
        set
        {
            _fontName = value;// "Tahoma"; //value;
            _font = null;
        }
    }
    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _font = null;
        }
    }
    public LingoTextStyle FontStyle { get; set; }
    public AColor TextColor { get; set; } = AColor.FromRGB(0, 0, 0);
    public AbstTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }
    public int Width { 
        get => _width; 
        set => _width = value; }
    public int Height { get; set; }
    public int TextWidth => _textWidth;
    public int TextHeight => _textHeight;


    protected SdlMemberTextBase(IAbstFontManager fontManager, ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
        _fontManager = (SdlFontManager)fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
    }
    public void Dispose()
    {
        if (_surface != nint.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = nint.Zero;
        }
        if (_font != null)
            _font.Release();
        _font = null;
    }
    public void Copy(string text) => SdlClipboard.SetText(text);
    public string PasteClipboard() => SdlClipboard.GetText();
    public string ReadText() => File.Exists(_lingoMemberText.FileName) ? File.ReadAllText(_lingoMemberText.FileName) : string.Empty;
    public string ReadTextRtf()
    {
        var rtf = Path.ChangeExtension(_lingoMemberText.FileName, ".rtf");
        return File.Exists(rtf) ? File.ReadAllText(rtf) : string.Empty;
    }
    public void CopyToClipboard() => SdlClipboard.SetText(Text);
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() => _lingoMemberText.Text = SdlClipboard.GetText();
    public void Preload() { IsLoaded = true; }
    public void Unload()
    {
        IsLoaded = false;
        if (_surface != nint.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = nint.Zero;
        }
        if (_font != null)
            _font.Release();
        _font = null;
    }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        PreloadFont();

        if (_font == null || string.IsNullOrWhiteSpace(_text))
            return null;
        var font = _font.FontHandle;
        if (font == nint.Zero) return null;
        var text = Text ?? string.Empty;
        // decide final box
        if (_renderedText != text)
            MeasureText(text);
        float boxW = Width > 0 ? Width : _textWidth;
        float boxH = Height > 0 ? Height : _textHeight;
        var (tex, textW, textH) = AbstSdlLabel.CreateTextTextureBox(_sdlRootContext.Renderer, font, FontSize, text, (int)boxW, (int)boxH, Alignment, TextColor.ToSDLColor());
        _surface = tex;
        SDL.SDL_SetTextureBlendMode(_surface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        
        // update sizes
        if (Width <= 0) Width = (int)boxW;
        if (Height <= 0) Height = (int)boxH;

        _measuredWidth = textW;
        _renderedText = Text;
        _lastWidth = Width; 
        _lastHeight = Height;
        _lingoMemberText.RegPoint = new APoint(-Width/2, -Height/2);
       
        // make a copy
        //var copyTextureSDL = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), (int)boxW, (int)boxH);
        return new SdlTexture2D(_surface, (int)boxW, (int)boxH);
    }

    private void PreloadFont()
    {
        if (_font == null)
            _font = _fontManager.GetTyped(this, FontName,FontSize);
    }

   

    /// <summary>Returns the bounding box (w,h) for multiline text (handles \n). No wrapping.</summary>
    private void MeasureText(string text)
    {
        var (maxW, totalH) = AbstSdlLabel.MeasureSDLText(_font!.FontHandle, text);
        _textWidth = maxW;
        _textHeight = totalH;
    }

}
