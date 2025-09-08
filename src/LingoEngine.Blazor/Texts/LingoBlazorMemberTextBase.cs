using System;
using System.IO;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Blazor.Primitives;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Texts;

/// <summary>
/// Basic Blazor implementation for text-based cast members. It renders the
/// text onto an off-screen canvas using JavaScript and exposes it as a texture
/// for the Lingo runtime.
/// </summary>
public abstract class LingoBlazorMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private AbstBlazorTexture2D? _texture;
    private bool _dirty;

    private string _text = string.Empty;
    private bool _wordWrap;
    private int _scrollTop;
    private string _fontName = string.Empty;
    private int _fontSize = 12;
    private LingoTextStyle _fontStyle;
    private AColor _textColor = AColor.FromRGB(0, 0, 0);
    private AbstTextAlignment _alignment;
    private int _margin;
    private int _width;
    private int _height;

    public LingoBlazorMemberTextBase(IJSRuntime js, AbstUIScriptResolver scripts)
    {
        _js = js;
        _scripts = scripts;
    }

    internal void Init(TText member) => _lingoMemberText = member;

    public bool IsDirty => _dirty;
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _dirty = true;
            }
        }
    }
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap != value)
            {
                _wordWrap = value;
                _dirty = true;
            }
        }
    }
    public int ScrollTop
    {
        get => _scrollTop;
        set
        {
            if (_scrollTop != value)
            {
                _scrollTop = value;
                _dirty = true;
            }
        }
    }
    public string FontName
    {
        get => _fontName;
        set
        {
            if (_fontName != value)
            {
                _fontName = value;
                _dirty = true;
            }
        }
    }
    public int FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                _dirty = true;
            }
        }
    }
    public LingoTextStyle FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                _dirty = true;
            }
        }
    }
    public AColor TextColor
    {
        get => _textColor;
        set
        {
            if (!_textColor.Equals(value))
            {
                _textColor = value;
                _dirty = true;
            }
        }
    }
    public AbstTextAlignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment != value)
            {
                _alignment = value;
                _dirty = true;
            }
        }
    }
    public int Margin
    {
        get => _margin;
        set
        {
            if (_margin != value)
            {
                _margin = value;
                _dirty = true;
            }
        }
    }
    public bool IsLoaded { get; private set; }
    public int Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                _dirty = true;
            }
        }
    }
    public int Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                _dirty = true;
            }
        }
    }
    public IAbstTexture2D? TextureLingo => _texture;

    public IAbstFontManager FontManager => throw new NotImplementedException();

    public void Copy(string text) => _js.InvokeVoidAsync("navigator.clipboard.writeText", text).AsTask().GetAwaiter().GetResult();

    public string PasteClipboard() => _js.InvokeAsync<string>("navigator.clipboard.readText").AsTask().GetAwaiter().GetResult();


    public void CopyToClipboard() => Copy(Text);

    public void Erase()
    {
        Unload();
        Text = string.Empty;
    }

    public void ImportFileInto() { }

    public void PasteClipboardInto() => Text = PasteClipboard();

    public void Preload() => IsLoaded = true;

    public void Unload()
    {
        IsLoaded = false;
        _texture?.Dispose();
        _texture = null;
    }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public bool IsPixelTransparent(int x, int y) => false;

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        int lines = Math.Max(1, Text.Split('\n').Length);
        int w = Width > 0 ? Width : Math.Max(1, Text.Length * FontSize / 2 + Margin * 2);
        int h = Height > 0 ? Height : FontSize * lines + Margin * 2;
        Width = w;
        Height = h;

        _texture?.Dispose();
        _texture = AbstBlazorTexture2D.CreateAsync(_js, w, h).GetAwaiter().GetResult();
        var ctx = _scripts.CanvasGetContext(_texture.Canvas, false).GetAwaiter().GetResult();

        string color = TextColor.ToCss();
        string align = Alignment switch
        {
            AbstTextAlignment.Center => "center",
            AbstTextAlignment.Right => "right",
            _ => "left"
        };
        _scripts.CanvasDrawText(ctx, Margin, Margin + FontSize, Text, FontName, color, FontSize, align).GetAwaiter().GetResult();
        IsLoaded = true;
        _dirty = false;
        return _texture;
    }

    public void Dispose() => Unload();
}

