using Microsoft.AspNetCore.Components;
using AbstUI.Components.Containers;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorLayoutWrapperComponent : AbstBlazorComponentModelBase, IAbstFrameworkLayoutWrapper
{
    public RenderFragment? ContentFragment { get; }

    public AbstBlazorLayoutWrapperComponent(AbstLayoutWrapper layoutWrapper)
    {
        if (layoutWrapper.Content.FrameworkObj is AbstBlazorComponentBase component)
            ContentFragment = component.RenderFragment;
    }
}
