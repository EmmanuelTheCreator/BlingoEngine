using System.Collections.Generic;
using System.Numerics;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Components;
using FluentAssertions;
using Xunit;

namespace AbstUI.SDLTest;

public class WindowRenderingOrderTests
{
    [Fact]
    public void PanelRendersBehindWindow()
    {
        var focus = new SdlFocusManager();
        var container = new AbstSDLComponentContainer(focus);
        var order = new List<string>();

        var panelComp = new ContainerComponent("panel", order);
        var panelCtx = new AbstSDLComponentContext(container, panelComp);

        var wrapperComp = new ContainerComponent("wrapper", order);
        var wrapperCtx = new AbstSDLComponentContext(container, wrapperComp);

        var panelButtonComp = new LeafComponent("panelButton", order);
        var panelButtonCtx = new AbstSDLComponentContext(container, panelButtonComp);

        wrapperComp.AddChild(panelButtonCtx);
        panelComp.AddChild(wrapperCtx);

        wrapperCtx.SetParents(panelCtx);
        panelButtonCtx.SetParents(wrapperCtx);

        var windowComp = new ContainerComponent("window", order);
        var windowCtx = new AbstSDLComponentContext(container, windowComp);

        var windowButtonComp = new LeafComponent("windowButton", order);
        var windowButtonCtx = new AbstSDLComponentContext(container, windowButtonComp);

        windowComp.AddChild(windowButtonCtx);
        windowButtonCtx.SetParents(windowCtx);

        panelCtx.SetZIndex(0);
        windowCtx.SetZIndex(10);

        var renderCtx = new AbstSDLRenderContext(nint.Zero, Vector2.Zero, new SdlFontManager(), null);

        container.Render(renderCtx);

        order.Should().Equal(new[] { "panel", "wrapper", "panelButton", "window", "windowButton" });
    }

    private sealed class ContainerComponent : IAbstSDLComponent
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

    private sealed class LeafComponent : IAbstSDLComponent
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
}

