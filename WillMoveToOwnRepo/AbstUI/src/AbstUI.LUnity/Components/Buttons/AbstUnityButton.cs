using System;
using AbstUI.Primitives;
using AbstUI.LUnity.Bitmaps;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Components.Buttons;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;

namespace AbstUI.LUnity.Components.Buttons;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkButton"/>.
/// </summary>
internal class AbstUnityButton : AbstUnityComponent, IAbstFrameworkButton, IFrameworkFor<AbstButton>
{
    private readonly Button _button;
    private readonly Image _image;
    private readonly Text _textComponent;
    private string _text = string.Empty;
    private IAbstTexture2D? _iconTexture;


    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _textComponent.text = value;
        }
    }

    public bool Enabled
    {
        get => _button.interactable;
        set => _button.interactable = value;
    }

    public IAbstTexture2D? IconTexture
    {
        get => _iconTexture;
        set
        {
            _iconTexture = value;
            _image.sprite = value is UnityTexture2D tex ? tex.ToSprite() : null;
        }
    }

    public AColor BorderColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public AColor BackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public AColor BackgroundHoverColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public AColor TextColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action? Pressed;
    public AbstUnityButton() : base(CreateGameObject(out var textComponent))
    {
        _textComponent = textComponent;
        _button = GameObject.GetComponent<Button>();
        _image = GameObject.GetComponent<Image>();
        _button.onClick.AddListener(() => Pressed?.Invoke());
    }

    private static GameObject CreateGameObject(out Text text)
    {
        var go = new GameObject("Button");
        go.AddComponent<Image>();
        go.AddComponent<Button>();
        var textGo = new GameObject("Text");
        textGo.transform.parent = go.transform;
        text = textGo.AddComponent<Text>();
        return go;
    }

    public void Invoke() => _button.onClick.Invoke();
}
