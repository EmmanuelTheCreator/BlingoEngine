using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Containers;

namespace AbstUI.Blazor.Components.Containers;

public partial class AbstBlazorLayoutWrapper : IAbstFrameworkLayoutWrapper
{
    private readonly RenderFragment? _contentFragment;
    private readonly AbstLayoutWrapper _lingoLayoutWrapper;
    public AbstBlazorLayoutWrapper(AbstLayoutWrapper layoutWrapper)
    {
        _lingoLayoutWrapper = layoutWrapper;
        _lingoLayoutWrapper.Init(this);

        if (layoutWrapper.Content.FrameworkObj is AbstBlazorComponentBase component)
            _contentFragment = component.RenderFragment;
    }

    private string BuildWrapperStyle()
    {
        var style = base.BuildStyle();
        style += $"position:absolute;left:{X}px;top:{Y}px;";
        return style;
    }
}
