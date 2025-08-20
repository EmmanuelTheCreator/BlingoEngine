using System;
using AbstUI.Components.Inputs;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputText"/>.
/// </summary>
internal class AbstUnityInputText : AbstUnityComponent, IAbstFrameworkInputText
{
    private readonly InputField _inputField;
    private readonly Text _textComponent;
    private string _text = string.Empty;
    private AColor _textColor = new(0, 0, 0);

    public AbstUnityInputText() : base(CreateGameObject(out var input, out var text))
    {
        _inputField = input;
        _textComponent = text;
        _inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private static GameObject CreateGameObject(out InputField input, out Text text)
    {
        var go = new GameObject("InputField");
        go.AddComponent<Image>();
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

    public bool IsMultiLine
    {
        get => _inputField.lineType != InputField.LineType.SingleLine;
        set => _inputField.lineType = value ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
    }

    public event Action? ValueChanged;
}
