using System;
using AbstUI.Components.Buttons;
using AbstUI.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Buttons;

public class AbstBlazorButtonComponent : IAbstFrameworkButton, IFrameworkFor<AbstButton>
{
    public event Action? Pressed;
    public event Action? Changed;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; Changed?.Invoke(); } }
    }

    private bool _visibility = true;
    public bool Visibility
    {
        get => _visibility;
        set { if (_visibility != value) { _visibility = value; Changed?.Invoke(); } }
    }

    private float _width;
    public float Width
    {
        get => _width;
        set { if (Math.Abs(_width - value) > float.Epsilon) { _width = value; Changed?.Invoke(); } }
    }

    private float _height;
    public float Height
    {
        get => _height;
        set { if (Math.Abs(_height - value) > float.Epsilon) { _height = value; Changed?.Invoke(); } }
    }

    private AMargin _margin;
    public AMargin Margin
    {
        get => _margin;
        set { _margin = value; Changed?.Invoke(); }
    }

    private AColor _borderColor;
    public AColor BorderColor
    {
        get => _borderColor;
        set { if (!_borderColor.Equals(value)) { _borderColor = value; Changed?.Invoke(); } }
    }

    private AColor _backgroundColor = AColors.White;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set { if (!_backgroundColor.Equals(value)) { _backgroundColor = value; Changed?.Invoke(); } }
    }

    private AColor _backgroundHoverColor;
    public AColor BackgroundHoverColor
    {
        get => _backgroundHoverColor;
        set { if (!_backgroundHoverColor.Equals(value)) { _backgroundHoverColor = value; Changed?.Invoke(); } }
    }

    private AColor _textColor = AColors.Black;
    public AColor TextColor
    {
        get => _textColor;
        set { if (!_textColor.Equals(value)) { _textColor = value; Changed?.Invoke(); } }
    }

    public object FrameworkNode => this;

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; Changed?.Invoke(); } }
    }

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; Changed?.Invoke(); } }
    }

    public IAbstTexture2D? IconTexture { get; set; }

    public void RaisePressed() => Pressed?.Invoke();

    public void Dispose() { }
}
