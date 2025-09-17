using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDLTest.Fakes;

internal sealed class CompositionTestRootContext : ISdlRootComponentContext
{
    public CompositionTestRootContext(nint renderer, SdlFocusManager focusManager)
    {
        Renderer = renderer;
        FocusManager = focusManager;
        ComponentContainer = new AbstSDLComponentContainer(focusManager);
    }

    public AbstSDLComponentContainer ComponentContainer { get; }

    public nint Renderer { get; }

    public IAbstMouse AbstMouse { get; } = new CompositionDummyMouse();

    public IAbstKey AbstKey { get; } = new CompositionDummyKey();

    public SdlFocusManager FocusManager { get; }

    public APoint GetWindowSize() => new(800, 600);
}

file sealed class CompositionDummyMouse : IAbstMouse
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

    public AMouseCursor GetCursor() => AMouseCursor.Arrow;

    public void SetCursor(AMouseCursor cursor)
    {
    }
}

file sealed class CompositionDummyKey : IAbstKey
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
