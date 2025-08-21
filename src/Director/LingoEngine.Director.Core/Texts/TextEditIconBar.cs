using System;
using System.Collections.Generic;
using System.Drawing;
using AbstUI.Texts;
using AbstUI.Primitives;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Texts;
using AbstUI.Components.Inputs;
using AbstUI.Components.Containers;
using AbstUI.Components.Buttons;

namespace LingoEngine.Director.Core.Texts;

/// <summary>
/// Reusable toolbar for editing text formatting properties.
/// Creates all UI elements through the framework factory and
/// exposes strongly typed events for all value changes.
/// </summary>
public class TextEditIconBar
{
    private readonly AbstInputCombobox _stylesCombo;
    private readonly AbstButton _addStyleButton;
    private readonly AbstButton _removeStyleButton;
    private readonly AbstStateButton _alignLeft;
    private readonly AbstStateButton _alignCenter;
    private readonly AbstStateButton _alignRight;
    private readonly AbstStateButton _alignJustified;
    private readonly AbstStateButton _boldButton;
    private readonly AbstStateButton _italicButton;
    private readonly AbstStateButton _underlineButton;
    private readonly AbstPanel _colorDisplay;
    private readonly AbstColorPicker _colorPicker;
    private readonly AbstInputSpinBox _fontSize;
    private readonly AbstInputSpinBox _lineHeight;
    private readonly AbstInputSpinBox _marginLeft;
    private readonly AbstInputSpinBox _marginRight;
    private readonly AbstInputCombobox _fontsCombo;

    private readonly Dictionary<string, AbstTextStyle> _styles = new();
    private AbstTextStyle _currentStyle;

    private const string DefaultStyleName = "Default";

    /// <summary>Container panel holding the toolbar items.</summary>
    public AbstPanel Panel { get; }

    /// <summary>Raised when the text alignment changes.</summary>
    public event Action<AbstTextAlignment>? AlignmentChanged;
    /// <summary>Raised when bold style is toggled.</summary>
    public event Action<bool>? BoldChanged;
    /// <summary>Raised when italic style is toggled.</summary>
    public event Action<bool>? ItalicChanged;
    /// <summary>Raised when underline style is toggled.</summary>
    public event Action<bool>? UnderlineChanged;
    /// <summary>Raised when font size changes.</summary>
    public event Action<int>? FontSizeChanged;
    /// <summary>Raised when a font is selected.</summary>
    public event Action<string>? FontChanged;
    /// <summary>Raised when color changes.</summary>
    public event Action<AColor>? ColorChanged;
    /// <summary>Raised when line height changes.</summary>
    public event Action<int>? LineHeightChanged;
    /// <summary>Raised when left margin changes.</summary>
    public event Action<int>? MarginLeftChanged;
    /// <summary>Raised when right margin changes.</summary>
    public event Action<int>? MarginRightChanged;

    /// <summary>Returns whether bold is currently selected.</summary>
    public bool IsBold => _boldButton.IsOn;
    /// <summary>Returns whether italic is currently selected.</summary>
    public bool IsItalic => _italicButton.IsOn;
    /// <summary>Returns whether underline is currently selected.</summary>
    public bool IsUnderline => _underlineButton.IsOn;
    /// <summary>Currently selected font name.</summary>
    public string SelectedFont => _fontsCombo.SelectedValue ?? string.Empty;

    public TextEditIconBar(ILingoFrameworkFactory factory)
    {
        const int actionBarHeight = 22;

        // Ensure default style exists
        var defaultStyle = new AbstTextStyle
        {
            Name = DefaultStyleName,
            FontSize = 12,
            Font = string.Empty,
            Color = AColors.Black,
            Alignment = AbstTextAlignment.Left,
            Bold = false,
            Italic = false,
            Underline = false,
            LineHeight = 0,
            MarginLeft = 0,
            MarginRight = 0
        };
        _styles.Add(defaultStyle.Name, defaultStyle);
        _currentStyle = defaultStyle;

        Panel = factory.CreatePanel("TextEditIconBar");
        Panel.Height = actionBarHeight;

        var container = factory.CreateWrapPanel(AOrientation.Horizontal, "TextEditIconBarContainer");
        Panel.AddItem(container);

        _stylesCombo = factory.CreateInputCombobox("TextStyles", s =>
        {
            if (s != null) ApplyStyle(s);
        });
        _stylesCombo.Width = 100;
        container.AddItem(_stylesCombo);

        _addStyleButton = factory.CreateButton("AddTextStyle", "+");
        _addStyleButton.Width = 20;
        _addStyleButton.Pressed += AddStyle;
        container.AddItem(_addStyleButton);

        _removeStyleButton = factory.CreateButton("RemoveTextStyle", "-");
        _removeStyleButton.Width = 20;
        _removeStyleButton.Pressed += RemoveCurrentStyle;
        container.AddItem(_removeStyleButton);

        _alignLeft = factory.CreateStateButton("AlignLeft", null, "L", _ =>
        {
            _currentStyle.Alignment = AbstTextAlignment.Left;
            AlignmentChanged?.Invoke(AbstTextAlignment.Left);
        });
        _alignCenter = factory.CreateStateButton("AlignCenter", null, "C", _ =>
        {
            _currentStyle.Alignment = AbstTextAlignment.Center;
            AlignmentChanged?.Invoke(AbstTextAlignment.Center);
        });
        _alignRight = factory.CreateStateButton("AlignRight", null, "R", _ =>
        {
            _currentStyle.Alignment = AbstTextAlignment.Right;
            AlignmentChanged?.Invoke(AbstTextAlignment.Right);
        });
        _alignJustified = factory.CreateStateButton("AlignJustified", null, "J", _ =>
        {
            _currentStyle.Alignment = AbstTextAlignment.Justified;
            AlignmentChanged?.Invoke(AbstTextAlignment.Justified);
        });
        container.AddItem(_alignLeft);
        container.AddItem(_alignCenter);
        container.AddItem(_alignRight);
        container.AddItem(_alignJustified);

        _boldButton = factory.CreateStateButton("Bold", null, "B", v =>
        {
            _currentStyle.Bold = v;
            BoldChanged?.Invoke(v);
        });
        _italicButton = factory.CreateStateButton("Italic", null, "I", v =>
        {
            _currentStyle.Italic = v;
            ItalicChanged?.Invoke(v);
        });
        _underlineButton = factory.CreateStateButton("Underline", null, "U", v =>
        {
            _currentStyle.Underline = v;
            UnderlineChanged?.Invoke(v);
        });
        container.AddItem(_boldButton);
        container.AddItem(_italicButton);
        container.AddItem(_underlineButton);

        _fontSize = factory.CreateSpinBox("FontSize", 1, 200, v =>
        {
            _currentStyle.FontSize = (int)v;
            FontSizeChanged?.Invoke((int)v);
        });
        _fontSize.Width = 50;
        container.AddItem(_fontSize);

        _lineHeight = factory.CreateSpinBox("LineHeight", 0, 500, v =>
        {
            _currentStyle.LineHeight = (int)v;
            LineHeightChanged?.Invoke((int)v);
        });
        _lineHeight.Width = 50;
        container.AddItem(_lineHeight);

        _marginLeft = factory.CreateSpinBox("MarginLeft", 0, 500, v =>
        {
            _currentStyle.MarginLeft = (int)v;
            MarginLeftChanged?.Invoke((int)v);
        });
        _marginLeft.Width = 50;
        container.AddItem(_marginLeft);

        _marginRight = factory.CreateSpinBox("MarginRight", 0, 500, v =>
        {
            _currentStyle.MarginRight = (int)v;
            MarginRightChanged?.Invoke((int)v);
        });
        _marginRight.Width = 50;
        container.AddItem(_marginRight);

        _fontsCombo = factory.CreateInputCombobox("FontsCombo", s =>
        {
            if (s != null)
            {
                _currentStyle.Font = s;
                FontChanged?.Invoke(s);
            }
        });
        _fontsCombo.Width = 100;
        container.AddItem(_fontsCombo);

        _colorDisplay = factory.CreatePanel("ColorDisplay");
        _colorDisplay.Width = actionBarHeight;
        _colorDisplay.Height = actionBarHeight;
        container.AddItem(_colorDisplay);

        _colorPicker = factory.CreateColorPicker("ColorPicker", c =>
        {
            _currentStyle.Color = c;
            _colorDisplay.BackgroundColor = c;
            ColorChanged?.Invoke(c);
        });
        _colorPicker.Width = actionBarHeight;
        _colorPicker.Height = actionBarHeight;
        container.AddItem(_colorPicker);

        RefreshStylesCombo();
        _stylesCombo.SelectedKey = DefaultStyleName;
    }

    /// <summary>Populate fonts available for selection.</summary>
    public void SetFonts(IEnumerable<string> fonts)
    {
        _fontsCombo.ClearItems();
        foreach (var font in fonts)
            _fontsCombo.AddItem(font, font);
    }

    /// <summary>Set the currently selected font.</summary>
    public void SetFont(string font)
    {
        _fontsCombo.SelectedKey = font;
        _currentStyle.Font = font;
    }

    /// <summary>Set current font size.</summary>
    public void SetFontSize(int size)
    {
        _fontSize.Value = size;
        _currentStyle.FontSize = size;
    }

    /// <summary>Set the current line height.</summary>
    public void SetLineHeight(int value)
    {
        _lineHeight.Value = value;
        _currentStyle.LineHeight = value;
    }

    /// <summary>Set the left margin.</summary>
    public void SetMarginLeft(int value)
    {
        _marginLeft.Value = value;
        _currentStyle.MarginLeft = value;
    }

    /// <summary>Set the right margin.</summary>
    public void SetMarginRight(int value)
    {
        _marginRight.Value = value;
        _currentStyle.MarginRight = value;
    }

    /// <summary>Set the alignment state.</summary>
    public void SetAlignment(AbstTextAlignment alignment)
    {
        _alignLeft.IsOn = alignment == AbstTextAlignment.Left;
        _alignCenter.IsOn = alignment == AbstTextAlignment.Center;
        _alignRight.IsOn = alignment == AbstTextAlignment.Right;
        _alignJustified.IsOn = alignment == AbstTextAlignment.Justified;
        _currentStyle.Alignment = alignment;
    }

    public void SetBold(bool on)
    {
        _boldButton.IsOn = on;
        _currentStyle.Bold = on;
    }

    public void SetItalic(bool on)
    {
        _italicButton.IsOn = on;
        _currentStyle.Italic = on;
    }

    public void SetUnderline(bool on)
    {
        _underlineButton.IsOn = on;
        _currentStyle.Underline = on;
    }

    /// <summary>Set the current font color.</summary>
    public void SetColor(AColor color)
    {
        _colorDisplay.BackgroundColor = color;
        _colorPicker.Color = color;
        _currentStyle.Color = color;
    }

    /// <summary>
    /// Update all UI controls to reflect the values of the given text member.
    /// </summary>
    public void SetMemberValues(ILingoMemberTextBase member)
    {
        SetFontSize(member.FontSize);
        SetFont(member.Font);
        SetColor(member.TextColor);
        SetAlignment(member.Alignment);
        SetBold(member.Bold);
        SetItalic(member.Italic);
        SetUnderline(member.Underline);
        SetLineHeight(0);
        SetMarginLeft(0);
        SetMarginRight(0);

        // Update default style from member
        var style = _styles[DefaultStyleName];
        style.FontSize = member.FontSize;
        style.Font = member.Font;
        style.Color = member.TextColor;
        style.Alignment = member.Alignment;
        style.Bold = member.Bold;
        style.Italic = member.Italic;
        style.Underline = member.Underline;
        style.LineHeight = 0;
        style.MarginLeft = 0;
        style.MarginRight = 0;

        ApplyStyle(DefaultStyleName);
        _stylesCombo.SelectedKey = DefaultStyleName;
    }

    public void OnResizing(float x, float y)
    {
        Panel.Width = x;
    }

    private void ApplyStyle(string name)
    {
        if (_styles.TryGetValue(name, out var style))
        {
            _currentStyle = style;
            SetFontSize(style.FontSize);
            SetFont(style.Font);
            SetColor(style.Color);
            SetAlignment(style.Alignment);
            SetBold(style.Bold);
            SetItalic(style.Italic);
            SetUnderline(style.Underline);
            SetLineHeight(style.LineHeight);
            SetMarginLeft(style.MarginLeft);
            SetMarginRight(style.MarginRight);
        }
    }

    private void AddStyle()
    {
        string newName = GenerateStyleName();
        var style = new AbstTextStyle
        {
            Name = newName,
            FontSize = _currentStyle.FontSize,
            Font = _currentStyle.Font,
            Color = _currentStyle.Color,
            Alignment = _currentStyle.Alignment,
            Bold = _currentStyle.Bold,
            Italic = _currentStyle.Italic,
            Underline = _currentStyle.Underline,
            LineHeight = _currentStyle.LineHeight,
            MarginLeft = _currentStyle.MarginLeft,
            MarginRight = _currentStyle.MarginRight
        };
        _styles.Add(newName, style);
        RefreshStylesCombo();
        ApplyStyle(newName);
        _stylesCombo.SelectedKey = newName;
    }

    private void RemoveCurrentStyle()
    {
        if (_currentStyle.Name == DefaultStyleName) return;
        _styles.Remove(_currentStyle.Name);
        RefreshStylesCombo();
        ApplyStyle(DefaultStyleName);
        _stylesCombo.SelectedKey = DefaultStyleName;
    }

    private void RefreshStylesCombo()
    {
        _stylesCombo.ClearItems();
        foreach (var style in _styles.Values)
            _stylesCombo.AddItem(style.Name, style.Name);
    }

    private string GenerateStyleName()
    {
        int i = 1;
        string name;
        do
        {
            name = $"Style{i++}";
        } while (_styles.ContainsKey(name));
        return name;
    }
}

