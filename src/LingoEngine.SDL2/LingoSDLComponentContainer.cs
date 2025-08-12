using System.Collections.Generic;
namespace LingoEngine.SDL2;

public class LingoSDLComponentContainer
{
    private readonly List<LingoSDLComponentContext> _allComponents = new();
    private readonly HashSet<LingoSDLComponentContext> _activeComponents = new();

    internal void Register(LingoSDLComponentContext context) => _allComponents.Add(context);
    internal void Unregister(LingoSDLComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(LingoSDLComponentContext context) => _activeComponents.Add(context);
    public void Deactivate(LingoSDLComponentContext context) => _activeComponents.Remove(context);

    public void Render(LingoSDLRenderContext renderContext)
    {
        foreach (var ctx in _activeComponents)
        {
            ctx.RenderToTexture(renderContext);
        }
    }
}
