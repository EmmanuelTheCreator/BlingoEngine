using System;
using System.Collections.Generic;
using System.Drawing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.Director.Core.Texts;

/// <summary>
/// Reusable toolbar for editing text formatting properties.
/// Creates all UI elements through the framework factory and
/// exposes strongly typed events for all value changes.
/// </summary>
public class TextEditIconBar
{
    private readonly LingoGfxStateButton _alignLeft;
    private readonly LingoGfxStateButton _alignCenter;
    private readonly LingoGfxStateButton _alignRight;
    private readonly LingoGfxStateButton _alignJustified;
    private readonly LingoGfxStateButton _boldButton;
    private readonly LingoGfxStateButton _italicButton;
    private readonly LingoGfxStateButton _underlineButton;
    private readonly LingoGfxPanel _colorDisplay;
    private readonly LingoGfxColorPicker _colorPicker;
    private readonly LingoGfxSpinBox _fontSize;
    private readonly LingoGfxInputCombobox _fontsCombo;

    /// <summary>Container panel holding the toolbar items.</summary>
    public LingoGfxPanel Panel { get; }

    /// <summary>Raised when the text alignment changes.</summary>
    public event Action<LingoTextAlignment>? AlignmentChanged;
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
    public event Action<LingoColor>? ColorChanged;

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

        Panel = factory.CreatePanel("TextEditIconBar");
        Panel.Height = actionBarHeight;

        var container = factory.CreateWrapPanel(LingoOrientation.Horizontal, "TextEditIconBarContainer");
        Panel.AddItem(container);

        _alignLeft = factory.CreateStateButton("AlignLeft", null, "L", _ => AlignmentChanged?.Invoke(LingoTextAlignment.Left));
        _alignCenter = factory.CreateStateButton("AlignCenter", null, "C", _ => AlignmentChanged?.Invoke(LingoTextAlignment.Center));
        _alignRight = factory.CreateStateButton("AlignRight", null, "R", _ => AlignmentChanged?.Invoke(LingoTextAlignment.Right));
        _alignJustified = factory.CreateStateButton("AlignJustified", null, "J", _ => AlignmentChanged?.Invoke(LingoTextAlignment.Justified));
        container.AddItem(_alignLeft);
        container.AddItem(_alignCenter);
        container.AddItem(_alignRight);
        container.AddItem(_alignJustified);

        _boldButton = factory.CreateStateButton("Bold", null, "B", v => BoldChanged?.Invoke(v));
        _italicButton = factory.CreateStateButton("Italic", null, "I", v => ItalicChanged?.Invoke(v));
        _underlineButton = factory.CreateStateButton("Underline", null, "U", v => UnderlineChanged?.Invoke(v));
        container.AddItem(_boldButton);
        container.AddItem(_italicButton);
        container.AddItem(_underlineButton);

        _fontSize = factory.CreateSpinBox("FontSize", 1, 200, v => FontSizeChanged?.Invoke((int)v));
        _fontSize.Width = 50;
        container.AddItem(_fontSize);

        _fontsCombo = factory.CreateInputCombobox("FontsCombo", s => { if (s != null) FontChanged?.Invoke(s); });
        _fontsCombo.Width = 100;
        container.AddItem(_fontsCombo);

        _colorDisplay = factory.CreatePanel("ColorDisplay");
        _colorDisplay.Width = actionBarHeight;
        _colorDisplay.Height = actionBarHeight;
        container.AddItem(_colorDisplay);

        _colorPicker = factory.CreateColorPicker("ColorPicker", c =>
        {
            _colorDisplay.BackgroundColor = c;
            ColorChanged?.Invoke(c);
        });
        _colorPicker.Width = actionBarHeight;
        _colorPicker.Height = actionBarHeight;
        container.AddItem(_colorPicker);
    }

    /// <summary>Populate fonts available for selection.</summary>
    public void SetFonts(IEnumerable<string> fonts)
    {
        _fontsCombo.ClearItems();
        foreach (var font in fonts)
            _fontsCombo.AddItem(font, font);
    }

    /// <summary>Set the currently selected font.</summary>
    public void SetFont(string font) => _fontsCombo.SelectedKey = font;

    /// <summary>Set current font size.</summary>
    public void SetFontSize(int size) => _fontSize.Value = size;

    /// <summary>Set the alignment state.</summary>
    public void SetAlignment(LingoTextAlignment alignment)
    {
        _alignLeft.IsOn = alignment == LingoTextAlignment.Left;
        _alignCenter.IsOn = alignment == LingoTextAlignment.Center;
        _alignRight.IsOn = alignment == LingoTextAlignment.Right;
        _alignJustified.IsOn = alignment == LingoTextAlignment.Justified;
    }

    public void SetBold(bool on) => _boldButton.IsOn = on;
    public void SetItalic(bool on) => _italicButton.IsOn = on;
    public void SetUnderline(bool on) => _underlineButton.IsOn = on;

    /// <summary>Set the current font color.</summary>
    public void SetColor(LingoColor color)
    {
        _colorDisplay.BackgroundColor = color;
        _colorPicker.Color = color;
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
    }

    public void OnResizing(float x, float y)
    {
        Panel.Width = x;
    }
}

