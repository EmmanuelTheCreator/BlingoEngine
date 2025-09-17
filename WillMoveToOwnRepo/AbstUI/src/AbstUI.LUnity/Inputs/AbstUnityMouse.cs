using AbstUI.Inputs;
using AbstUI.Primitives;
using UnityEngine;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Inputs;

public class AbstUnityMouse<TAbstMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<TAbstMouseType>
        where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<TAbstMouseType> _blingoMouse;
    private bool _hidden;

    private AMouseCursor _cursor = AMouseCursor.Arrow;

    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    public AbstUnityMouse(Lazy<TAbstMouseType> mouse)
    {
        _blingoMouse = mouse;
    }

    public void SetMouseObj(TAbstMouseType mouse) => _blingoMouse = new Lazy<TAbstMouseType>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
    }

    public void SetOffset(int x, int y)
    {
        OffsetX = x;
        OffsetY = y;
    }

    public void SetCursor(AMouseCursor value)
    {
        _cursor = value;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    public AMouseCursor GetCursor() => _cursor;

    public void Release()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ReplaceMouseObj(IAbstMouse blingoMouse) => _blingoMouse = new Lazy<TAbstMouseType>(() => (TAbstMouseType)blingoMouse);

    public void HandleMouseMove(int x, int y)
    {
        _blingoMouse.Value.MouseH = x;
        _blingoMouse.Value.MouseV = y;
        _blingoMouse.Value.DoMouseMove();
    }

    public void HandleMouseDown(int button, bool doubleClick = false)
    {
        switch (button)
        {
            case 0:
                _blingoMouse.Value.MouseDown = true;
                _blingoMouse.Value.LeftMouseDown = true;
                _blingoMouse.Value.DoubleClick = doubleClick;
                break;
            case 1:
                _blingoMouse.Value.RightMouseDown = true;
                break;
            case 2:
                _blingoMouse.Value.MiddleMouseDown = true;
                break;
        }
        _blingoMouse.Value.DoMouseDown();
    }

    public void HandleMouseUp(int button)
    {
        switch (button)
        {
            case 0:
                _blingoMouse.Value.MouseDown = false;
                _blingoMouse.Value.LeftMouseDown = false;
                break;
            case 1:
                _blingoMouse.Value.RightMouseDown = false;
                break;
            case 2:
                _blingoMouse.Value.MiddleMouseDown = false;
                break;
        }
        _blingoMouse.Value.DoMouseUp();
    }

    public void HandleMouseWheel(float delta)
    {
        _blingoMouse.Value.DoMouseWheel(delta);
    }

    
}

public class AbstUnityMouse : AbstUnityMouse<AbstMouse, AbstMouseEvent>
{
    private static AbstMouse? _mouse;

    public AbstUnityMouse()
        : base(new Lazy<AbstMouse>(() => _mouse!))
    {
    }

    public new void ReplaceMouseObj(IAbstMouse blingoMouse)
    {
        _mouse = (AbstMouse)blingoMouse;
        base.ReplaceMouseObj(blingoMouse);
    }
}

