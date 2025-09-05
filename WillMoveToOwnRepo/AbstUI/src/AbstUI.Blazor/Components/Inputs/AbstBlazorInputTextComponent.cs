using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputTextComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputText, IFrameworkFor<AbstInputText>, IHasTextBackgroundBorderColor
{
    private string _text = string.Empty;
    private int _caret;
    private int _selectionStart = -1;
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

    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set { if (!_backgroundColor.Equals(value)) { _backgroundColor = value; RaiseChanged(); } }
    }

    private AColor _borderColor = AbstDefaultColors.InputBorderColor;
    public AColor BorderColor
    {
        get => _borderColor;
        set { if (!_borderColor.Equals(value)) { _borderColor = value; RaiseChanged(); } }
    }

    private bool _isMultiLine;
    public bool IsMultiLine
    {
        get => _isMultiLine;
        set { if (_isMultiLine != value) { _isMultiLine = value; RaiseChanged(); } }
    }

    public bool HasSelection => _selectionStart != -1 && _selectionStart != _caret;

    public void DeleteSelection()
    {
        if (!HasSelection) return;
        int start = Math.Min(_selectionStart, _caret);
        int end = Math.Max(_selectionStart, _caret);
        _text = _text.Remove(start, end - start);
        _caret = start;
        _selectionStart = -1;
        RaiseChanged();
        RaiseValueChanged();
    }

    public void SetCaretPosition(int position)
    {
        _caret = Math.Clamp(position, 0, _text.Length);
        _selectionStart = -1;
    }

    public int GetCaretPosition() => _caret;

    public void SetSelection(int start, int end)
    {
        _selectionStart = Math.Clamp(start, 0, _text.Length);
        _caret = Math.Clamp(end, 0, _text.Length);
        if (_selectionStart == _caret)
            _selectionStart = -1;
    }

    public void SetSelection(Range range)
    {
        SetSelection(range.Start.GetOffset(_text.Length), range.End.GetOffset(_text.Length));
    }

    public void InsertText(string text)
    {
        if (HasSelection)
            DeleteSelection();
        _text = _text.Insert(_caret, text);
        _caret += text.Length;
        RaiseChanged();
        RaiseValueChanged();
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
