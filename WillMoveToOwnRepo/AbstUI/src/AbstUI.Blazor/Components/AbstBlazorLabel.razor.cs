using Microsoft.AspNetCore.Components;
using AbstUI.Texts;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorLabel : AbstBlazorComponentBase
{
    private AbstBlazorLabelComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorLabelComponent Component { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _component = Component;
        SyncFromComponent();
        _component.Changed += OnComponentChanged;
    }

    private void OnComponentChanged()
    {
        SyncFromComponent();
        RequestRender();
    }

    private void SyncFromComponent()
    {
        Visibility = _component.Visibility;
        Width = _component.Width;
        Height = _component.Height;
        Margin = _component.Margin;
    }

    private string BuildSpanStyle()
    {
        var style = BuildStyle();
        style += $"color:{_component.FontColor.ToHex()};font-size:{_component.FontSize}px;text-align:{GetAlignment()};";
        return style;
    }

    private string GetAlignment() => _component.TextAlignment switch
    {
        AbstTextAlignment.Center => "center",
        AbstTextAlignment.Right => "right",
        _ => "left"
    };

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
