using System.Numerics;
using AbstUI.Components.Containers;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Core;
using AbstUI.Styles;
using AbstUI.SDLTest.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AbstUI.SDLTest;

public class TabContainerCompositionTests
{
    [Fact]
    public void DeeplyNestedCompositionMaintainsParentContexts()
    {
        SdlTestHost.Run((window, renderer, fontManager) =>
        {
            var focus = new SdlFocusManager();
            var rootContext = new CompositionTestRootContext(renderer, focus);
            var services = new ServiceCollection()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstFontManager>(_ => fontManager)
                .BuildServiceProvider();

            var factory = new AbstSdlComponentFactory(rootContext, services);

            var windowPanel = factory.CreatePanel("window");
            var innerPanel = factory.CreatePanel("inner");
            var tabContainer = factory.CreateTabContainer("tabs");
            var tabItem = factory.CreateTabItem("cast", "Cast");
            var tabContent = factory.CreatePanel("tabContent");
            var wrapPanel = factory.CreateWrapPanel(AOrientation.Horizontal, "wrap");
            var childOne = factory.CreatePanel("childOne");
            var childTwo = factory.CreatePanel("childTwo");

            wrapPanel.AddItem(childOne);
            wrapPanel.AddItem(childTwo);
            tabContent.AddItem(wrapPanel);
            tabItem.Content = tabContent;
            tabContainer.AddTab(tabItem);
            innerPanel.AddItem(tabContainer);
            windowPanel.AddItem(innerPanel);

            var windowImpl = windowPanel.Framework<AbstSdlPanel>();
            var innerImpl = innerPanel.Framework<AbstSdlPanel>();
            var tabsImpl = tabContainer.Framework<AbstSdlTabContainer>();
            var tabItemImpl = tabItem.Framework<AbstSdlTabItem>();
            var tabContentImpl = tabContent.Framework<AbstSdlPanel>();
            var wrapImpl = wrapPanel.Framework<AbstSdlWrapPanel>();
            var childOneImpl = childOne.Framework<AbstSdlPanel>();
            var childTwoImpl = childTwo.Framework<AbstSdlPanel>();

            tabsImpl.ComponentContext.VisualParent.Should().Be(innerImpl.ComponentContext);
            tabItemImpl.ComponentContext.VisualParent.Should().Be(tabsImpl.ComponentContext);
            tabContentImpl.ComponentContext.VisualParent.Should().Be(tabItemImpl.ComponentContext);
            wrapImpl.ComponentContext.VisualParent.Should().Be(tabContentImpl.ComponentContext);
            childOneImpl.ComponentContext.VisualParent.Should().Be(wrapImpl.ComponentContext);
            childTwoImpl.ComponentContext.VisualParent.Should().Be(wrapImpl.ComponentContext);
        });
    }

    [Fact]
    public void DeeplyNestedCompositionRendersWithNestedGeometry()
    {
        SdlTestHost.Run((window, renderer, fontManager) =>
        {
            var focus = new SdlFocusManager();
            var rootContext = new CompositionTestRootContext(renderer, focus);
            var services = new ServiceCollection()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstFontManager>(_ => fontManager)
                .BuildServiceProvider();

            var factory = new AbstSdlComponentFactory(rootContext, services);

            var windowPanel = factory.CreatePanel("window");
            var innerPanel = factory.CreatePanel("inner");
            var tabContainer = factory.CreateTabContainer("tabs");
            var tabItem = factory.CreateTabItem("cast", "Cast");
            var tabContent = new ProbePanel(factory, "tabContent");
            var wrapPanel = new ProbeWrapPanel(factory, AOrientation.Horizontal, "wrap");
            var childOne = new ProbePanel(factory, "childOne");
            var childTwo = new ProbePanel(factory, "childTwo");

            var tabsImpl = tabContainer.Framework<AbstSdlTabContainer>();

            wrapPanel.AddItem(childOne);
            wrapPanel.AddItem(childTwo);
            tabContent.AddItem(wrapPanel);
            tabItem.Content = tabContent;
            tabContainer.AddTab(tabItem);
            innerPanel.AddItem(tabContainer);
            windowPanel.AddItem(innerPanel);

            windowPanel.Width = 420;
            windowPanel.Height = 320;

            innerPanel.X = 12;
            innerPanel.Y = 16;
            innerPanel.Width = 360;
            innerPanel.Height = 260;

            tabContainer.X = 18;
            tabContainer.Y = 20;
            tabContainer.Width = 300;
            tabContainer.Height = 180;
            tabsImpl.BorderThickness = 3;

            wrapPanel.X = 8;
            wrapPanel.Y = 6;
            wrapPanel.Width = 280;
            wrapPanel.Height = 120;
            wrapPanel.ItemMargin = new APoint(5, 4);

            childOne.Width = 90;
            childOne.Height = 40;
            childOne.Margin = new AMargin(4, 6, 2, 3);

            childTwo.Width = 70;
            childTwo.Height = 50;
            childTwo.Margin = new AMargin(3, 2, 5, 4);

            var windowImpl = windowPanel.Framework<AbstSdlPanel>();
            windowImpl.ComponentContext.SetZIndex(0);

            var renderCtx = factory.CreateRenderContext(null, Vector2.Zero);

            windowImpl.ComponentContext.RenderToTexture(renderCtx);

            var tabSnapshot = tabContent.Impl.RenderSnapshots.Should().ContainSingle().Subject;
            tabSnapshot.OffsetX.Should().Be(tabsImpl.BorderThickness);
            var tabHeaderHeight = tabSnapshot.OffsetY - tabsImpl.BorderThickness;
            tabHeaderHeight.Should().BeGreaterThan(0);
            tabContent.Width.Should().Be(tabContainer.Width - tabsImpl.BorderThickness * 2);
            tabContent.Height.Should().Be(tabContainer.Height - tabHeaderHeight - tabsImpl.BorderThickness * 2);

            var wrapSnapshot = wrapPanel.Impl.RenderSnapshots.Should().ContainSingle().Subject;
            wrapSnapshot.DrawX.Should().BeApproximately(wrapPanel.X, 0.0001f);
            wrapSnapshot.DrawY.Should().BeApproximately(wrapPanel.Y, 0.0001f);

            var childOneSnapshot = childOne.Impl.RenderSnapshots.Should().ContainSingle().Subject;
            childOneSnapshot.DrawX.Should().BeApproximately(childOne.Margin.Left, 0.0001f);
            childOneSnapshot.DrawY.Should().BeApproximately(childOne.Margin.Top, 0.0001f);

            var childTwoSnapshot = childTwo.Impl.RenderSnapshots.Should().ContainSingle().Subject;
            childTwoSnapshot.DrawX.Should().BeApproximately(
                childOneSnapshot.DrawX + childOne.Width + childOne.Margin.Right + wrapPanel.ItemMargin.X + childTwo.Margin.Left,
                0.0001f);
            childTwoSnapshot.DrawY.Should().BeApproximately(childTwo.Margin.Top, 0.0001f);

            float tabLeft = tabSnapshot.OffsetX;
            float tabTop = tabSnapshot.OffsetY;
            float tabRight = tabLeft + tabContent.Width;
            float tabBottom = tabTop + tabContent.Height;

            float wrapLeft = tabLeft + wrapSnapshot.DrawX;
            float wrapTop = tabTop + wrapSnapshot.DrawY;
            float wrapRight = wrapLeft + wrapPanel.Width;
            float wrapBottom = wrapTop + wrapPanel.Height;

            wrapLeft.Should().BeGreaterThanOrEqualTo(tabLeft);
            wrapTop.Should().BeGreaterThanOrEqualTo(tabTop);
            wrapRight.Should().BeLessThanOrEqualTo(tabRight);
            wrapBottom.Should().BeLessThanOrEqualTo(tabBottom);

            float childOneGlobalLeft = wrapLeft + childOneSnapshot.DrawX;
            float childOneGlobalTop = wrapTop + childOneSnapshot.DrawY;
            float childOneGlobalRight = childOneGlobalLeft + childOne.Width;
            float childOneGlobalBottom = childOneGlobalTop + childOne.Height;

            childOneGlobalLeft.Should().BeGreaterThanOrEqualTo(tabLeft);
            childOneGlobalTop.Should().BeGreaterThanOrEqualTo(tabTop);
            childOneGlobalRight.Should().BeLessThanOrEqualTo(tabRight);
            childOneGlobalBottom.Should().BeLessThanOrEqualTo(tabBottom);

            float childTwoGlobalLeft = wrapLeft + childTwoSnapshot.DrawX;
            float childTwoGlobalTop = wrapTop + childTwoSnapshot.DrawY;
            float childTwoGlobalRight = childTwoGlobalLeft + childTwo.Width;
            float childTwoGlobalBottom = childTwoGlobalTop + childTwo.Height;

            childTwoGlobalLeft.Should().BeGreaterThanOrEqualTo(tabLeft);
            childTwoGlobalTop.Should().BeGreaterThanOrEqualTo(tabTop);
            childTwoGlobalRight.Should().BeLessThanOrEqualTo(tabRight);
            childTwoGlobalBottom.Should().BeLessThanOrEqualTo(tabBottom);
        });
    }


}
