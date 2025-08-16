using System.Collections.Generic;
namespace AbstUI.ImGui;

public class AbstImGuiComponentContainer
{
    private readonly List<AbstImGuiComponentContext> _allComponents = new();
    private readonly HashSet<AbstImGuiComponentContext> _activeComponents = new();

    internal void Register(AbstImGuiComponentContext context) => _allComponents.Add(context);
    internal void Unregister(AbstImGuiComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(AbstImGuiComponentContext context) => _activeComponents.Add(context);
    public void Deactivate(AbstImGuiComponentContext context) => _activeComponents.Remove(context);

    public void Render(AbstImGuiRenderContext renderContext)
    {
        foreach (var ctx in _activeComponents)
        {
            ctx.RenderToTexture(renderContext);
        }
    }
}
