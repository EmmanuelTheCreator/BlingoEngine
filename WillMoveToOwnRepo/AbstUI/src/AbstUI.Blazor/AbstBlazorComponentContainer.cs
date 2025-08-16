using System.Collections.Generic;
namespace AbstUI.Blazor;

public class AbstBlazorComponentContainer
{
    private readonly List<AbstBlazorComponentContext> _allComponents = new();
    private readonly HashSet<AbstBlazorComponentContext> _activeComponents = new();

    internal void Register(AbstBlazorComponentContext context) => _allComponents.Add(context);
    internal void Unregister(AbstBlazorComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(AbstBlazorComponentContext context) => _activeComponents.Add(context);
    public void Deactivate(AbstBlazorComponentContext context) => _activeComponents.Remove(context);

    public void Render(AbstBlazorRenderContext renderContext)
    {
    }
}

