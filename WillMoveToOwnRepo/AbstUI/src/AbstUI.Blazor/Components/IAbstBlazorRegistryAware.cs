using AbstUI.Blazor;

namespace AbstUI.Blazor.Components;

public interface IAbstBlazorRegistryAware
{
    void AttachRegistry(AbstBlazorComponentContainer registry);
}
