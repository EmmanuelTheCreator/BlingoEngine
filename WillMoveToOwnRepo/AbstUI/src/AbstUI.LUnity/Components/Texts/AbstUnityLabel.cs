using AbstUI.Components.Texts;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using AbstUI.Texts;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Components.Texts;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkLabel"/>.
/// </summary>
internal class AbstUnityLabel : AbstUnityComponent, IAbstFrameworkLabel, IFrameworkFor<AbstLabel>
{
    private readonly Text _textComponent;
    private string _text = string.Empty;
    private int _fontSize = 12;
    private AColor _fontColor = new(255, 255, 255);
    private ATextWrapMode _wrapMode = ATextWrapMode.Off;
    private AbstTextAlignment _textAlignment = AbstTextAlignment.Left;

    public AbstUnityLabel() : base(CreateGameObject(out var text))
    {
        _textComponent = text;
    }

    private static GameObject CreateGameObject(out Text text)
    {
        var go = new GameObject("Text");
        text = go.AddComponent<Text>();
        return go;
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _textComponent.text = value;
        }
    }

    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _textComponent.fontSize = value;
        }
    }

    public string? Font { get; set; }

    public AColor FontColor
    {
        get => _fontColor;
        set
        {
            _fontColor = value;
            _textComponent.color = value.ToUnityColor();
        }
    }

    public int LineHeight { get; set; } = 1;

    public ATextWrapMode WrapMode
    {
        get => _wrapMode;
        set
        {
            _wrapMode = value;
            _textComponent.horizontalOverflow = value == ATextWrapMode.Off
                ? HorizontalWrapMode.Overflow
                : HorizontalWrapMode.Wrap;
        }
    }

    public AbstTextAlignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            _textAlignment = value;
            _textComponent.alignment = value switch
            {
                AbstTextAlignment.Left => TextAnchor.MiddleLeft,
                AbstTextAlignment.Center => TextAnchor.MiddleCenter,
                AbstTextAlignment.Right => TextAnchor.MiddleRight,
                _ => TextAnchor.MiddleLeft,
            };
        }
    }
}
