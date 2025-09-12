using System;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static AbstUI.SDL2.SDLL.SDL;
using AbstUI.Styles;

namespace AbstUI.SDLTest;

public class ScrollZoomWrapPanelMouseEventTests
{
    [Fact]
    public void ButtonInsideNestedContainersReceivesClick()
    {
        SdlTestHost.Run((window, renderer, fontManager) =>
        {
            var focus = new SdlFocusManager();
            var root = new TestRootContext(renderer, focus);
            var services = new ServiceCollection()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstFontManager>(_ => fontManager)
                .BuildServiceProvider();
            var factory = new AbstSdlComponentFactory(root, services);

            var scroll = factory.CreateScrollContainer("scroll");
            scroll.Width = 100;
            scroll.Height = 100;
            scroll.ScrollHorizontal = 4;
            scroll.ScrollVertical = 6;

            var zoom = factory.CreateZoomBox("zoom");
            zoom.Width = 100;
            zoom.Height = 100;
            zoom.ScaleH = 2;
            zoom.ScaleV = 2;

            var wrap = factory.CreateWrapPanel(AOrientation.Horizontal, "wrap");
            wrap.Width = 100;
            wrap.Height = 100;

            var button = new TestButtonComponent(factory)
            {
                Width = 50,
                Height = 50,
            };
            wrap.AddItem(button);
            zoom.Content = wrap;
            zoom.OffsetX = 5;
            zoom.OffsetY = 8;
            scroll.AddItem(zoom);

            var scrollImpl = scroll.Framework<AbstSdlScrollContainer>();
            var zoomImpl = zoom.Framework<AbstSdlZoomBox>();
            var wrapImpl = wrap.Framework<AbstSdlWrapPanel>();

            scrollImpl.ZIndex = 0;
            zoomImpl.ZIndex = 0;
            wrapImpl.ZIndex = 0;
            button.ZIndex = 0;

            var sdlEvent = new SDL_Event
            {
                type = SDL_EventType.SDL_MOUSEBUTTONDOWN,
                button = new SDL_MouseButtonEvent
                {
                    type = SDL_EventType.SDL_MOUSEBUTTONDOWN,
                    button = 1,
                    x = 0,
                    y = 0,
                }
            };
            var evt = new AbstSDLEvent(sdlEvent)
            {
                OffsetX = 11,
                OffsetY = 10,
            };

            scrollImpl.HandleEvent(evt);

            button.Clicked.Should().BeTrue();
            button.LastLeft.Should().Be(10);
            button.LastTop.Should().Be(12);
        });
    }

    private sealed class TestButtonComponent : AbstSdlComponent, IAbstFrameworkLayoutNode, IHandleSdlEvent
    {
        public TestButtonComponent(AbstSdlComponentFactory factory) : base(factory) { }

        public AMargin Margin { get; set; }
        public object FrameworkNode => this;
        public bool Clicked { get; private set; }
        public float LastLeft { get; private set; }
        public float LastTop { get; private set; }

        public bool CanHandleEvent(AbstSDLEvent e) => e.IsInside || !e.HasCoordinates;

        public void HandleEvent(AbstSDLEvent e)
        {
            Clicked = true;
            LastLeft = e.ComponentLeft;
            LastTop = e.ComponentTop;
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
            => AbstSDLRenderResult.RequireRender();
    }

    private sealed class TestRootContext : ISdlRootComponentContext
    {
        public TestRootContext(nint renderer, SdlFocusManager focus)
        {
            Renderer = renderer;
            FocusManager = focus;
            ComponentContainer = new AbstSDLComponentContainer(focus);
        }

        public AbstSDLComponentContainer ComponentContainer { get; }
        public nint Renderer { get; }
        public IAbstMouse AbstMouse { get; } = new DummyMouse();
        public IAbstKey AbstKey { get; } = new DummyKey();
        public SdlFocusManager FocusManager { get; }
        public APoint GetWindowSize() => new(800, 600);
    }

    private sealed class DummyMouse : IAbstMouse
    {
        public bool DoubleClick => false;
        public char MouseChar => '\0';
        public bool MouseDown => false;
        public float MouseH => 0;
        public int MouseLine => 0;
        public APoint MouseLoc => new();
        public bool MouseUp => false;
        public float MouseV => 0;
        public string MouseWord => string.Empty;
        public bool RightMouseDown => false;
        public bool RightMouseUp => false;
        public bool StillDown => false;
        public bool LeftMouseDown => false;
        public bool MiddleMouseDown => false;
        public float WheelDelta { get; set; }
        public IAbstMouse CreateNewInstance(IAbstMouseRectProvider provider) => this;
        public void SetCursor(AMouseCursor cursor) { }
    }

    private sealed class DummyKey : IAbstKey
    {
        public bool CommandDown => false;
        public bool ControlDown => false;
        public string Key => string.Empty;
        public int KeyCode => 0;
        public bool OptionDown => false;
        public bool ShiftDown => false;
        public bool KeyPressed(char key) => false;
        public bool KeyPressed(AbstUIKeyType key) => false;
        public bool KeyPressed(int keyCode) => false;
        public AbstKey Subscribe(IAbstKeyEventHandler<AbstKeyEvent> handler) => new();
        public AbstKey Unsubscribe(IAbstKeyEventHandler<AbstKeyEvent> handler) => new();
        public IAbstKey CreateNewInstance(IAbstActivationProvider provider) => this;
    }
}
