using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using RmlUiNet;

namespace AbstUI.SDL2RmlUi.Components;

/// <summary>
/// Single-line or multi-line text input implemented with RmlUi elements.
/// </summary>
public class RmlUiInputText : IAbstFrameworkInputText, IHasTextBackgroundBorderColor, IDisposable
{
    private readonly ElementDocument _document;
    private Element _element;
    private ElementFormControlInput? _input;
    private ElementFormControlTextArea? _textarea;
    private AMargin _margin;
    private string _name = string.Empty;
    private bool _visibility = true;
    private float _width;
    private float _height;
    private float _x;
    private float _y;
    private bool _enabled = true;
    private string _text = string.Empty;
    private int _maxLength;
    private string? _font;
    private int _fontSize;
    private AColor _textColor = AColors.Black;
    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    private AColor _borderColor = AbstDefaultColors.InputBorderColor;
    private bool _isMultiLine;
    private event Action? _valueChanged;

    public RmlUiInputText(ElementDocument document, bool multiLine)
    {
        _document = document;
        _isMultiLine = multiLine;
        InitElement(_isMultiLine);
    }

    private void InitElement(bool multiLine)
    {
        // remove existing element if reinitializing
        if (_element != null)
        {
            var parent = _element.GetParentNode();
            parent?.RemoveChild(_element);
        }
        _input = null;
        _textarea = null;

        if (multiLine)
        {
            _textarea = (ElementFormControlTextArea)_document.AppendChildTag("textarea");
            _element = _textarea;
        }
        else
        {
            _input = (ElementFormControlInput)_document.AppendChildTag("input");
            _element = _input;
        }

        // reapply stored properties
        if (!string.IsNullOrEmpty(_name))
            _element.SetAttribute("id", _name);
        _element.SetProperty("display", _visibility ? "block" : "none");
        _element.SetProperty("width", $"{_width}px");
        _element.SetProperty("height", $"{_height}px");
        _element.SetProperty("left", $"{_x}px");
        _element.SetProperty("top", $"{_y}px");
        _element.SetProperty("position", "absolute");
        _element.SetProperty("margin", $"{_margin.Top}px {_margin.Right}px {_margin.Bottom}px {_margin.Left}px");
        if (!_enabled) _element.SetAttribute("disabled", "disabled");
        if (_maxLength != 0) _element.SetAttribute("maxlength", _maxLength.ToString());
        if (!string.IsNullOrEmpty(_font)) _element.SetProperty("font-family", _font);
        if (_fontSize != 0) _element.SetProperty("font-size", $"{_fontSize}px");
        _element.SetProperty("color", _textColor.ToHex());
        _element.SetProperty("background-color", _backgroundColor.ToHex());
        _element.SetProperty("border-color", _borderColor.ToHex());

        if (_input != null) _input.SetValue(_text);
        else _textarea?.SetInnerRml(_text);

        _element.AddEventListener("change", _ => _valueChanged?.Invoke());
    }

    public object FrameworkNode => _element;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _element.SetAttribute("id", value);
        }
    }

    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            _element.SetProperty("display", value ? "block" : "none");
        }
    }

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            _element.SetProperty("left", $"{value}px");
            _element.SetProperty("position", "absolute");
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            _element.SetProperty("top", $"{value}px");
            _element.SetProperty("position", "absolute");
        }
    }

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            _element.SetProperty("width", $"{value}px");
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            _element.SetProperty("height", $"{value}px");
        }
    }

    public AMargin Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            _element.SetProperty("margin", $"{value.Top}px {value.Right}px {value.Bottom}px {value.Left}px");
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (value) _element.RemoveAttribute("disabled");
            else _element.SetAttribute("disabled", "disabled");
        }
    }

    public string Text
    {
        get => _input != null ? _input.GetValue() : _textarea?.GetInnerRml() ?? string.Empty;
        set
        {
            _text = value;
            if (_input != null) _input.SetValue(value);
            else _textarea?.SetInnerRml(value);
            _valueChanged?.Invoke();
        }
    }

    public int MaxLength
    {
        get => _maxLength;
        set
        {
            _maxLength = value;
            _element.SetAttribute("maxlength", value.ToString());
        }
    }

    public string? Font
    {
        get => _font;
        set
        {
            _font = value;
            _element.SetProperty("font-family", value ?? string.Empty);
        }
    }

    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _element.SetProperty("font-size", $"{value}px");
        }
    }

    public AColor TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            _element.SetProperty("color", value.ToHex());
        }
    }

    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _element.SetProperty("background-color", value.ToHex());
        }
    }

    public AColor BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            _element.SetProperty("border-color", value.ToHex());
        }
    }

    public bool IsMultiLine
    {
        get => _isMultiLine;
        set
        {
            if (_isMultiLine == value) return;
            _isMultiLine = value;
            InitElement(_isMultiLine);
        }
    }

    public event Action? ValueChanged
    {
        add => _valueChanged += value;
        remove => _valueChanged -= value;
    }

    public void Dispose() { }
}
