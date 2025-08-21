using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components.Graphics;

public partial class AbstBlazorGfxCanvas
{
    private AbstBlazorGfxCanvasComponent _component = default!;
    private ElementReference _canvasRef;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorGfxCanvasComponent Component { get; set; } = default!;

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

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        if (_component.Pixilated)
            style += "image-rendering:pixelated;";
        return style;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _component.SetCanvas(_canvasRef);
        await _component.OnAfterRenderAsync();
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
