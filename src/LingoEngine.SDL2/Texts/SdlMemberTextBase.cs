using System.Runtime.InteropServices;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Styles;
using LingoEngine.Sprites;
using LingoEngine.Styles;
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
    public SdlTexture2D? _textureSDL;
    private ISdlFontLoadedByUser? _font;
    private int fontSize;
    private string fontName = string.Empty;

    public SdlTexture2D? TextureSDL => _textureSDL;
    public ILingoTexture2D? TextureLingo => _textureSDL;
   

    public string Text { get => _text; set => _text = value; }
    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }
    public string FontName
    {
        get => fontName; 
        set
        {
            fontName = value;
            _font = null;
        }
    }
    public int FontSize
    {
        get => fontSize; 
        set
        {
            fontSize = value;
            _font = null;
        }
    }
    public LingoTextStyle FontStyle { get; set; }
    public AColor TextColor { get; set; } = AColor.FromRGB(0, 0, 0);
    public LingoTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }
    public int Width {get;set;}
    public int Height {get;set;}


    protected SdlMemberTextBase(ILingoFontManager fontManager, ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
        _fontManager = (SdlFontManager)fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
    }
    public void Dispose() {
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

    public ILingoTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        PreloadFont();

        if (_font == null)
            return null;
        var font = _font.FontHandle;
        if (font == nint.Zero) return null;


        var text = Text ?? string.Empty;
        var color = new SDL.SDL_Color { r = TextColor.R, g = TextColor.G, b = TextColor.B, a = TextColor.A };
        _surface = WordWrap
            ? SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(font, text, color, (uint)Math.Max(Width, 1))
            : SDL_ttf.TTF_RenderUTF8_Blended(font, text, color);

        if (_surface == nint.Zero)
            return null;
        
        var s = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
        Width = s.w;
        Height = s.h;
        _textureSDL = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), s.w, s.h);

        // make a copy
        var copyTextureSDL = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), s.w, s.h);
        return copyTextureSDL;
    }

    private void PreloadFont()
    {
        if (_font == null)
            _font = _fontManager.GetTyped(this, FontName, FontSize);
    }
}
