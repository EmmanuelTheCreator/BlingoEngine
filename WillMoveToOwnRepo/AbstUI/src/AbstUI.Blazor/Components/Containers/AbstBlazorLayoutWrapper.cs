using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorLayoutWrapper : AbstBlazorComponentBase, IAbstFrameworkLayoutWrapper
{
    private readonly RenderFragment? _contentFragment;
    private readonly AbstLayoutWrapper _lingoLayoutWrapper;

    public object FrameworkNode => this;

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
