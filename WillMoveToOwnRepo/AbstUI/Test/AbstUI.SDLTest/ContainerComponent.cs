using System.Collections.Generic;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;

namespace AbstUI.SDLTest;

internal sealed class ContainerComponent : IAbstSDLComponent
{
    private readonly string _name;
    private readonly List<string> _order;
    private readonly List<AbstSDLComponentContext> _children = new();

    public ContainerComponent(string name, List<string> order)
    {
        _name = name;
        _order = order;
    }

    public void AddChild(AbstSDLComponentContext child) => _children.Add(child);

    public AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        _order.Add(_name);
        foreach (var child in _children)
            child.RenderToTexture(context);
        return AbstSDLRenderResult.RequireRender();
    }
}
