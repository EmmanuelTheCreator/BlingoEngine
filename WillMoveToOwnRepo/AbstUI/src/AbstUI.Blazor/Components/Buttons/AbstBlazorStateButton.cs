using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Buttons;
using AbstUI.Blazor.Primitives;
using AbstUI.Styles;

namespace AbstUI.Blazor.Components.Buttons;

public partial class AbstBlazorStateButton : IAbstFrameworkStateButton
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public bool IsOn { get; set; }
    [Parameter] public AColor BorderColor { get; set; } = AbstDefaultColors.Button_Border_Normal;
    [Parameter] public AColor BorderHoverColor { get; set; } = AbstDefaultColors.Button_Border_Hover;
    [Parameter] public AColor BorderPressedColor { get; set; } = AbstDefaultColors.Button_Border_Pressed;
    [Parameter] public AColor BackgroundColor { get; set; } = AbstDefaultColors.Button_Bg_Normal;
    [Parameter] public AColor BackgroundHoverColor { get; set; } = AbstDefaultColors.Button_Bg_Hover;
    [Parameter] public AColor BackgroundPressedColor { get; set; } = AbstDefaultColors.Button_Bg_Pressed;
    [Parameter] public AColor TextColor { get; set; } = AColor.FromRGB(0, 0, 0);
    public IAbstTexture2D? TextureOn { get; set; }
    public IAbstTexture2D? TextureOff { get; set; }

    private bool _hover;
    private bool _pressed;

    private void HandleClick()
    {
        if (!Enabled) return;
        IsOn = !IsOn;
        ValueChangedInvoke();
    }

    private void HandleMouseOver() => _hover = true;
    private void HandleMouseOut() { _hover = false; _pressed = false; }
    private void HandleMouseDown() { if (Enabled) _pressed = true; }
    private void HandleMouseUp() => _pressed = false;

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        var border = _pressed || IsOn ? BorderPressedColor : _hover ? BorderHoverColor : BorderColor;
        var bg = _pressed || IsOn ? BackgroundPressedColor : _hover ? BackgroundHoverColor : BackgroundColor;
        style += $"border:1px solid {border.ToCss()};background:{bg.ToCss()};color:{TextColor.ToCss()};";
        return style;
    }
}
