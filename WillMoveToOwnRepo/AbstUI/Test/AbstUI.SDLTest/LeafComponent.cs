using System.Collections.Generic;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;

namespace AbstUI.SDLTest;

internal sealed class LeafComponent : IAbstSDLComponent
{
    private readonly string _name;
    private readonly List<string> _order;

    public LeafComponent(string name, List<string> order)
    {
        _name = name;
        _order = order;
    }

    public AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        _order.Add(_name);
        return AbstSDLRenderResult.RequireRender();
    }
}
