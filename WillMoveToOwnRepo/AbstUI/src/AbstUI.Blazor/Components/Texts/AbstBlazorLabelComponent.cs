using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Blazor.Components;

public class AbstBlazorLabelComponent : AbstBlazorComponentModelBase, IAbstFrameworkLabel
{
    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; RaiseChanged(); } }
    }

    private int _fontSize = 14;
    public int FontSize
    {
        get => _fontSize;
        set { if (_fontSize != value) { _fontSize = value; RaiseChanged(); } }
    }

    private string? _font;
    public string? Font
    {
        get => _font;
        set { if (_font != value) { _font = value; RaiseChanged(); } }
    }

    private AColor _fontColor = AColor.FromRGB(0, 0, 0);
    public AColor FontColor
    {
        get => _fontColor;
        set { if (!_fontColor.Equals(value)) { _fontColor = value; RaiseChanged(); } }
    }

    private int _lineHeight;
    public int LineHeight
    {
        get => _lineHeight;
        set { if (_lineHeight != value) { _lineHeight = value; RaiseChanged(); } }
    }

    private ATextWrapMode _wrapMode;
    public ATextWrapMode WrapMode
    {
        get => _wrapMode;
        set { if (_wrapMode != value) { _wrapMode = value; RaiseChanged(); } }
    }

    private AbstTextAlignment _textAlignment;
    public AbstTextAlignment TextAlignment
    {
        get => _textAlignment;
        set { if (_textAlignment != value) { _textAlignment = value; RaiseChanged(); } }
    }
}
