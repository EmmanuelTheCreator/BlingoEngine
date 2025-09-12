using System.Collections.Generic;
using System.Numerics;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Styles;
using FluentAssertions;
using Xunit;

namespace AbstUI.SDLTest;

public class ScrollContainerComboBoxPopupTests
{
    [Fact]
    public void ScrollContainerRendersBehindWindow()
    {
        var focus = new SdlFocusManager();
        var container = new AbstSDLComponentContainer(focus);
        var order = new List<string>();

        var scrollComp = new ContainerComponent("scroll", order);
        var scrollCtx = new AbstSDLComponentContext(container, scrollComp);

        var scrollButtonComp = new LeafComponent("scrollButton", order);
        var scrollButtonCtx = new AbstSDLComponentContext(container, scrollButtonComp);
        scrollComp.AddChild(scrollButtonCtx);
        scrollButtonCtx.SetParents(scrollCtx);

        var windowComp = new ContainerComponent("window", order);
        var windowCtx = new AbstSDLComponentContext(container, windowComp);

        var windowButtonComp = new LeafComponent("windowButton", order);
        var windowButtonCtx = new AbstSDLComponentContext(container, windowButtonComp);
        windowComp.AddChild(windowButtonCtx);
        windowButtonCtx.SetParents(windowCtx);

        scrollCtx.SetZIndex(0);
        windowCtx.SetZIndex(10);

        var renderCtx = new AbstSDLRenderContext(nint.Zero, Vector2.Zero, new SdlFontManager(), null);
        container.Render(renderCtx);

        order.Should().Equal(new[] { "scroll", "scrollButton", "window", "windowButton" });
    }

    [Fact]
    public void ComboboxPopupRendersOnTop()
    {
        var focus = new SdlFocusManager();
        var container = new AbstSDLComponentContainer(focus);
        var order = new List<string>();

        var scrollComp = new ContainerComponent("scroll", order);
        var scrollCtx = new AbstSDLComponentContext(container, scrollComp);

        var comboComp = new LeafComponent("combo", order);
        var comboCtx = new AbstSDLComponentContext(container, comboComp);
        scrollComp.AddChild(comboCtx);
        comboCtx.SetParents(scrollCtx);

        var popupComp = new LeafComponent("popup", order);
        var popupCtx = new AbstSDLComponentContext(container, popupComp) { AlwaysOnTop = true };

        scrollCtx.SetZIndex(0);
        popupCtx.SetZIndex(0);

        var renderCtx = new AbstSDLRenderContext(nint.Zero, Vector2.Zero, new SdlFontManager(), null);
        container.Render(renderCtx);

        order.Should().Equal(new[] { "scroll", "combo", "popup" });
    }
}

