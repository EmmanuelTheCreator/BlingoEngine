using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AbstUI.Blazor.Components.Containers;

public partial class AbstBlazorScrollContainer
{
    public string NameUnique { get; private set; }

    private AbstBlazorScrollContainerComponent _component = default!;
    private ElementReference myDiv;

    [Inject]
    AbstUIScriptResolver _scripts { get; set; } = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorScrollContainerComponent Component { get; set; } = default!;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        NameUnique = Guid.NewGuid().ToString();
        _component = Component;
        SyncFromComponent();
        _component.Changed += OnComponentChanged;
    }

    private void OnComponentChanged()
    {
        SyncFromComponent();
        RequestRender();
    }

    public async Task Scrolling(MouseEventArgs evt)
    {
        var scrollPosition = await _scripts.GetScrollPosition(NameUnique);
        _component.ScrollHorizontal = (float)scrollPosition.ScrollLeft;
        _component.ScrollVertical = (float)scrollPosition.ScrollTop;
    }

    private void SyncFromComponent()
    {
        Visibility = _component.Visibility;
        Width = _component.Width;
        Height = _component.Height;
        Margin = _component.Margin;
        X = _component.X;
        Y = _component.Y;
        Name = _component.Name;
    }

    private string BuildScrollStyle()
    {
        var style = $"position:absolute;left:{_component.X}px;top:{_component.Y}px;";
        style += _component.ClipContents ? "overflow:hidden;" : "overflow:auto;";
        return $"{BuildStyle()}{style}";
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
