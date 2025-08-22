using System;
using AbstUI.Components.Menus;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Menus;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkMenuItem"/>.
/// </summary>
internal class AbstUnityMenuItem : AbstUnityComponent, IAbstFrameworkMenuItem, IFrameworkFor<AbstMenuItem>
{
    private readonly Button _button;
    private readonly Text _text;

    public AbstUnityMenuItem(string name, string? shortcut)
        : base(CreateGameObject(name, out var button, out var text))
    {
        _button = button;
        _text = text;
        Shortcut = shortcut;
        _button.onClick.AddListener(() => Activated?.Invoke());
    }

    private static GameObject CreateGameObject(string name, out Button button, out Text text)
    {
        var go = new GameObject(name);
        button = go.AddComponent<Button>();
        var textGo = new GameObject("Text");
        textGo.transform.parent = go.transform;
        text = textGo.AddComponent<Text>();
        return go;
    }

    public new string Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            _text.text = value;
        }
    }

    public bool Enabled
    {
        get => _button.interactable;
        set => _button.interactable = value;
    }

    public bool CheckMark { get; set; }

    public string? Shortcut { get; set; }

    public event Action? Activated;
}
