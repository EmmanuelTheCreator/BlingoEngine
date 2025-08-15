using AbstUI.Inputs;
using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Inputs;

public class AbstUnityMouse<TLingoMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse
        where TLingoMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<TLingoMouseType> _lingoMouse;
    private bool _hidden;

    private AMouseCursor _cursor = AMouseCursor.Arrow;

    public AbstUnityMouse(Lazy<TLingoMouseType> mouse)
    {
        _lingoMouse = mouse;
    }

    public void SetMouseObj(TLingoMouseType mouse) => _lingoMouse = new Lazy<TLingoMouseType>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
    }



    public void SetCursor(AMouseCursor value)
    {
        _cursor = value;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Release()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ReplaceMouseObj(IAbstMouse lingoMouse) => _lingoMouse = new Lazy<TLingoMouseType>(() => (TLingoMouseType)lingoMouse);

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
