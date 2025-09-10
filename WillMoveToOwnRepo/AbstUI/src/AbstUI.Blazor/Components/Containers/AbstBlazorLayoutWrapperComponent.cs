using System;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Blazor;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorLayoutWrapperComponent : AbstBlazorComponentModelBase, IAbstFrameworkLayoutWrapper, IFrameworkFor<AbstLayoutWrapper>, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry;
    private readonly AbstBlazorComponentContainer _childContainer;
    private IAbstFrameworkNode? _content;

    public AbstBlazorLayoutWrapperComponent(AbstBlazorComponentContainer registry, AbstBlazorComponentMapper mapper)
    {
        _registry = registry;
        _childContainer = new AbstBlazorComponentContainer(mapper);
    }

    internal IAbstFrameworkNode? ContentFrameworkNode => _content;

    public AbstBlazorComponentContainer ChildContainer => _childContainer;

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        if (ReferenceEquals(_registry, registry))
            return;

        _registry = registry;
        if (_content != null)
        {
            if (_content is IAbstBlazorRegistryAware aware)
                aware.AttachRegistry(_registry);
            _registry.Register(_content);
        }
    }

    internal void SetContent(IAbstFrameworkNode content)
    {
        _content = content;
        if (_content is IAbstBlazorRegistryAware aware)
            aware.AttachRegistry(_registry);
        _registry.Register(_content);
        _childContainer.Register(_content);
    }

    public override void Dispose()
    {
        if (_content != null)
        {
            _childContainer.Unregister(_content);
            _registry.Unregister(_content);
        }
        base.Dispose();
    }
}
