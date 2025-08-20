using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public class AbstBlazorInputTextComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputText
{
    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; RaiseChanged(); } }
    }

    private int _maxLength;
    public int MaxLength
    {
        get => _maxLength;
        set { if (_maxLength != value) { _maxLength = value; RaiseChanged(); } }
    }

    private string? _font;
    public string? Font
    {
        get => _font;
        set { if (_font != value) { _font = value; RaiseChanged(); } }
    }

    private int _fontSize = 14;
    public int FontSize
    {
        get => _fontSize;
        set { if (_fontSize != value) { _fontSize = value; RaiseChanged(); } }
    }

    private AColor _textColor = AColors.Black;
    public AColor TextColor
    {
        get => _textColor;
        set { if (!_textColor.Equals(value)) { _textColor = value; RaiseChanged(); } }
    }

    private bool _isMultiLine;
    public bool IsMultiLine
    {
        get => _isMultiLine;
        set { if (_isMultiLine != value) { _isMultiLine = value; RaiseChanged(); } }
    }

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; RaiseChanged(); } }
    }

    public event Action? ValueChanged;
    public void RaiseValueChanged() => ValueChanged?.Invoke();
}
