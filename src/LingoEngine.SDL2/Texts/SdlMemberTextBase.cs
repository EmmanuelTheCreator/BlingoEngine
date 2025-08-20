using System.Runtime.InteropServices;
using AbstUI.Texts;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Sprites;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;

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
    public IAbstTexture2D? TextureLingo => _textureSDL;


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
    public AbstTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }
    public int Width { get; set; }
    public int Height { get; set; }


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

        if (_font == null)
            return null;
        var font = _font.FontHandle;
        if (font == nint.Zero) return null;


        var text = Text ?? string.Empty;
        var color = new SDL.SDL_Color { r = TextColor.R, g = TextColor.G, b = TextColor.B, a = TextColor.A };
        var hasLineBreak = text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0;
        var wrapLength = WordWrap ? (uint)Math.Max(Width, 1) : 0;

        _surface = WordWrap || hasLineBreak
            ? SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(font, text, color, wrapLength)
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
