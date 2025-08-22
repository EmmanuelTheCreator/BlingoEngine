using Microsoft.AspNetCore.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorLayoutWrapperComponent : AbstBlazorComponentModelBase, IAbstFrameworkLayoutWrapper, IFrameworkFor<AbstLayoutWrapper>
{
    public RenderFragment? ContentFragment { get; }

    public AbstBlazorLayoutWrapperComponent(AbstLayoutWrapper layoutWrapper)
    {
        if (layoutWrapper.Content.FrameworkObj is AbstBlazorComponentBase component)
            ContentFragment = component.RenderFragment;
    }
}
