using System;
using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using UnityEngine;

namespace AbstUI.GfxVisualTest.LUnity;

public sealed class UnityTestWindow : IFrameworkTestWindow
{
    // Fields
    private readonly GameObject _root;
    private readonly RectTransform _rect;
    private GfxTestWindow _instance = null!;
    private IAbstFrameworkNode? _content;
    private bool _isOpen;

    // Properties
    public string Title { get; set; } = string.Empty;
    public AColor BackgroundColor { get; set; } = AColors.White;
    public bool IsActiveWindow => _isOpen;
    public bool IsOpen => _isOpen;
    public IAbstMouse Mouse => _instance.Mouse;
    public IAbstKey AbstKey => _instance.Key;

    public IAbstFrameworkNode? Content
    {
        get => _content;
        set
        {
            _content = value;
            if (value?.FrameworkNode is GameObject go)
            {
                go.transform.SetParent(_root.transform, false);
            }
        }
    }

    public string Name
    {
        get => _root.name;
        set => _root.name = value;
    }

    public bool Visibility
    {
        get => _root.activeSelf;
        set => _root.SetActive(value);
    }

    public float Width
    {
        get => _rect.sizeDelta.x;
        set => _rect.sizeDelta = new Vector2(value, _rect.sizeDelta.y);
    }

    public float Height
    {
        get => _rect.sizeDelta.y;
        set => _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, value);
    }

    public AMargin Margin { get; set; }

    public object FrameworkNode => _root;

    // Constructors
    public UnityTestWindow()
    {
        _root = new GameObject(GfxTestWindow.MyWindowCode);
        _rect = _root.AddComponent<RectTransform>();
        _root.SetActive(false);
    }

    // Methods
    public void Init(GfxTestWindow instance)
    {
        _instance = instance;
    }

    void IFrameworkForInitializable<IAbstWindow>.Init(IAbstWindow window)
        => Init((GfxTestWindow)window);

    public void Dispose()
    {
        UnityEngine.Object.Destroy(_root);
    }

    public void OpenWindow()
    {
        _isOpen = true;
        Visibility = true;
    }

    public void CloseWindow()
    {
        _isOpen = false;
        Visibility = false;
    }

    public void MoveWindow(int x, int y)
    {
        _rect.anchoredPosition = new Vector2(x, -y);
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        MoveWindow(x, y);
        SetSize(width, height);
    }

    public APoint GetPosition()
    {
        var pos = _rect.anchoredPosition;
        return new APoint(pos.x, -pos.y);
    }

    public APoint GetSize() => new(Width, Height);

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
