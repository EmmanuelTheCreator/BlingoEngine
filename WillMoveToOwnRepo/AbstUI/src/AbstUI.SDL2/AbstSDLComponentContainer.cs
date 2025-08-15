using System.Collections.Generic;
namespace AbstUI.SDL2;

public class AbstSDLComponentContainer
{
    private readonly List<AbstSDLComponentContext> _allComponents = new();
    private readonly HashSet<AbstSDLComponentContext> _activeComponents = new();

    internal void Register(AbstSDLComponentContext context) => _allComponents.Add(context);
    internal void Unregister(AbstSDLComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(AbstSDLComponentContext context) => _activeComponents.Add(context);
    public void Deactivate(AbstSDLComponentContext context) => _activeComponents.Remove(context);

    public void Render(AbstSDLRenderContext renderContext)
    {
        foreach (var ctx in _activeComponents)
        {
            ctx.RenderToTexture(renderContext);
        }
    }
}
