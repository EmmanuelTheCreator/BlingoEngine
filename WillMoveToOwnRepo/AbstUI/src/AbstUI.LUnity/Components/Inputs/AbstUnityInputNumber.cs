using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Styles;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputNumber{TValue}"/>.
/// </summary>
internal class AbstUnityInputNumber<TValue> : AbstUnityComponent, IAbstFrameworkInputNumber<TValue>, IHasTextBackgroundBorderColor
    where TValue : struct, INumber<TValue>
{
    private readonly InputField _inputField;
    private readonly Text _textComponent;
    private readonly Image _image;
    private TValue _value = TValue.Zero;
    private string _currentText = string.Empty;
    private AColor _textColor = new(0, 0, 0);
    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    private AColor _borderColor = AbstDefaultColors.InputBorderColor;

    public AbstUnityInputNumber() : base(CreateGameObject(out var input, out var text, out var image))
    {
        _inputField = input;
        _textComponent = text;
        _image = image;
        _inputField.onValueChanged.AddListener(OnValueChanged);
        _image.color = _backgroundColor.ToUnityColor();
    }

    private static GameObject CreateGameObject(out InputField input, out Text text, out Image image)
    {
        var go = new GameObject("InputNumber");
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
        if (_currentText == value)
            return;
        _currentText = value;
        if (TValue.TryParse(value, CultureInfo.InvariantCulture, out var parsed))
        {
            parsed = TValue.Clamp(parsed, Min, Max);
            if (!EqualityComparer<TValue>.Default.Equals(_value, parsed))
            {
                _value = parsed;
                ValueChanged?.Invoke();
            }
            var text = _value.ToString(null, CultureInfo.InvariantCulture);
            if (_currentText != text)
            {
                _currentText = text;
                _inputField.text = text;
                _textComponent.text = text;
            }
        }
    }

    public bool Enabled
    {
        get => _inputField.interactable;
        set => _inputField.interactable = value;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            var v = TValue.Clamp(value, Min, Max);
            if (EqualityComparer<TValue>.Default.Equals(_value, v))
                return;
            _value = v;
            var text = v.ToString(null, CultureInfo.InvariantCulture);
            if (_currentText != text)
            {
                _currentText = text;
                _inputField.text = text;
                _textComponent.text = text;
            }
            ValueChanged?.Invoke();
        }
    }

    public TValue Min { get; set; } = TValue.Zero;
    public TValue Max { get; set; } = TValue.Zero;

    public ANumberType NumberType
    {
        get => _inputField.contentType == InputField.ContentType.IntegerNumber
            ? ANumberType.Integer
            : ANumberType.Float;
        set => _inputField.contentType = value == ANumberType.Integer
            ? InputField.ContentType.IntegerNumber
            : InputField.ContentType.DecimalNumber;
    }

    public int FontSize
    {
        get => _textComponent.fontSize;
        set => _textComponent.fontSize = value;
    }

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

    public event Action? ValueChanged;
}

