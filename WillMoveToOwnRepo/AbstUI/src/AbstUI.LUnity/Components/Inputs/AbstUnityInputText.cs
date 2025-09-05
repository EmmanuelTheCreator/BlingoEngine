using System;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Styles;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputText"/>.
/// </summary>
internal class AbstUnityInputText : AbstUnityComponent, IAbstFrameworkInputText, IFrameworkFor<AbstInputText>, IHasTextBackgroundBorderColor
{
    private readonly InputField _inputField;
    private readonly Text _textComponent;
    private readonly Image _image;
    private string _text = string.Empty;
    private AColor _textColor = new(0, 0, 0);
    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    private AColor _borderColor = AbstDefaultColors.InputBorderColor;

    public AbstUnityInputText() : base(CreateGameObject(out var input, out var text, out var image))
    {
        _inputField = input;
        _textComponent = text;
        _image = image;
        _inputField.onValueChanged.AddListener(OnValueChanged);
        _image.color = _backgroundColor.ToUnityColor();
    }

    private static GameObject CreateGameObject(out InputField input, out Text text, out Image image)
    {
        var go = new GameObject("InputField");
        image = go.AddComponent<Image>();
        input = go.AddComponent<InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.parent = go.transform;
        text = textGo.AddComponent<Text>();
        input.textComponent = text;
        return go;
    }

    private void OnValueChanged(string value)
    {
        if (_text == value) return;
        _text = value;
        _textComponent.text = value;
        ValueChanged?.Invoke();
    }

    public bool Enabled
    {
        get => _inputField.interactable;
        set => _inputField.interactable = value;
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            _inputField.text = value;
            _textComponent.text = value;
            ValueChanged?.Invoke();
        }
    }

    public int MaxLength
    {
        get => _inputField.characterLimit;
        set => _inputField.characterLimit = value;
    }

    public string? Font { get; set; }

    public int FontSize { get; set; }

    public AColor TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            _textComponent.color = value.ToUnityColor();
        }
    }

    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _image.color = value.ToUnityColor();
        }
    }

    public AColor BorderColor
    {
        get => _borderColor;
        set => _borderColor = value;
    }

    public bool IsMultiLine
    {
        get => _inputField.lineType != InputField.LineType.SingleLine;
        set => _inputField.lineType = value ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
    }

    public bool HasSelection => _inputField.selectionAnchorPosition != _inputField.selectionFocusPosition;

    public void DeleteSelection()
    {
        if (!HasSelection) return;
        int start = Math.Min(_inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);
        int end = Math.Max(_inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);
        Text = _text.Remove(start, end - start);
        SetCaretPosition(start);
    }

    public void SetCaretPosition(int position)
    {
        int pos = Math.Clamp(position, 0, _text.Length);
        _inputField.caretPosition = pos;
        _inputField.selectionAnchorPosition = pos;
        _inputField.selectionFocusPosition = pos;
    }

    public int GetCaretPosition() => _inputField.caretPosition;

    public void SetSelection(int start, int end)
    {
        int s = Math.Clamp(start, 0, _text.Length);
        int e = Math.Clamp(end, 0, _text.Length);
        _inputField.selectionAnchorPosition = s;
        _inputField.selectionFocusPosition = e;
        _inputField.caretPosition = e;
    }

    public void SetSelection(Range range)
    {
        SetSelection(range.Start.GetOffset(_text.Length), range.End.GetOffset(_text.Length));
    }

    public void InsertText(string text)
    {
        if (HasSelection)
            DeleteSelection();
        int pos = _inputField.caretPosition;
        Text = _text.Insert(pos, text);
        SetCaretPosition(pos + text.Length);
    }

    public event Action? ValueChanged;
}
