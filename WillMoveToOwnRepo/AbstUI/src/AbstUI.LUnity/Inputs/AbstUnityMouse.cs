using AbstUI.Inputs;
using AbstUI.Primitives;
using UnityEngine;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Inputs;

public class AbstUnityMouse<TAbstMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<TAbstMouseType>
        where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<TAbstMouseType> _lingoMouse;
    private bool _hidden;

    private AMouseCursor _cursor = AMouseCursor.Arrow;

    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    public AbstUnityMouse(Lazy<TAbstMouseType> mouse)
    {
        _lingoMouse = mouse;
    }

    public void SetMouseObj(TAbstMouseType mouse) => _lingoMouse = new Lazy<TAbstMouseType>(() => mouse);

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

    public void ReplaceMouseObj(IAbstMouse lingoMouse) => _lingoMouse = new Lazy<TAbstMouseType>(() => (TAbstMouseType)lingoMouse);

    public void HandleMouseMove(int x, int y)
    {
        _lingoMouse.Value.MouseH = x;
        _lingoMouse.Value.MouseV = y;
        _lingoMouse.Value.DoMouseMove();
    }

    public void HandleMouseDown(int button, bool doubleClick = false)
    {
        switch (button)
        {
            case 0:
                _lingoMouse.Value.MouseDown = true;
                _lingoMouse.Value.LeftMouseDown = true;
                _lingoMouse.Value.DoubleClick = doubleClick;
                break;
            case 1:
                _lingoMouse.Value.RightMouseDown = true;
                break;
            case 2:
                _lingoMouse.Value.MiddleMouseDown = true;
                break;
        }
        _lingoMouse.Value.DoMouseDown();
    }

    public void HandleMouseUp(int button)
    {
        switch (button)
        {
            case 0:
                _lingoMouse.Value.MouseDown = false;
                _lingoMouse.Value.LeftMouseDown = false;
                break;
            case 1:
                _lingoMouse.Value.RightMouseDown = false;
                break;
            case 2:
                _lingoMouse.Value.MiddleMouseDown = false;
                break;
        }
        _lingoMouse.Value.DoMouseUp();
    }

    public void HandleMouseWheel(float delta)
    {
        _lingoMouse.Value.DoMouseWheel(delta);
    }

    
}

public class AbstUnityMouse : AbstUnityMouse<AbstMouse, AbstMouseEvent>
{
    private static AbstMouse? _mouse;

    public AbstUnityMouse()
        : base(new Lazy<AbstMouse>(() => _mouse!))
    {
    }

    public new void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _mouse = (AbstMouse)lingoMouse;
        base.ReplaceMouseObj(lingoMouse);
    }
}
