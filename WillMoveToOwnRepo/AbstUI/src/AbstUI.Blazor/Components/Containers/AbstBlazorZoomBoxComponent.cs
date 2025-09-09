using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorZoomBoxComponent : AbstBlazorComponentModelBase, IAbstFrameworkZoomBox, IFrameworkFor<AbstZoomBox>, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry;
    private readonly AbstBlazorComponentContainer _childContainer;
    private IAbstFrameworkLayoutNode? _content;
    private float _scaleH = 1f;
    private float _scaleV = 1f;

    public AbstBlazorZoomBoxComponent(AbstBlazorComponentContainer registry, AbstBlazorComponentMapper mapper)
    {
        _registry = registry;
        _childContainer = new AbstBlazorComponentContainer(mapper);
    }

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        if (!ReferenceEquals(_registry, registry))
        {
            _registry = registry;
            if (_content != null)
            {
                if (_content is IAbstBlazorRegistryAware aware)
                    aware.AttachRegistry(_registry);
                _registry.Register(_content);
            }
        }
    }

    public AbstBlazorComponentContainer ChildContainer => _childContainer;

    public IAbstFrameworkLayoutNode? Content
    {
        get => _content;
        set
        {
            if (_content == value)
                return;
            if (_content != null)
            {
                _childContainer.Unregister(_content);
                _registry.Unregister(_content);
            }
            _content = value;
            if (_content != null)
            {
                if (_content is IAbstBlazorRegistryAware aware)
                    aware.AttachRegistry(_registry);
                _registry.Register(_content);
                _childContainer.Register(_content);
            }
            RaiseChanged();
        }
    }

    public float ScaleH
    {
        get => _scaleH;
        set
        {
            if (_scaleH == value)
                return;
            _scaleH = value;
            RaiseChanged();
        }
    }

    public float ScaleV
    {
        get => _scaleV;
        set
        {
            if (_scaleV == value)
                return;
            _scaleV = value;
            RaiseChanged();
        }
    }
}
