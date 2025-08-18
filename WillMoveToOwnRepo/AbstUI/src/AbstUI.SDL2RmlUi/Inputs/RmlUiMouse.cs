using System;
using AbstUI.Inputs;
using AbstUI.Primitives;
using RmlUiNet;
using RmlUiNet.Input;

namespace AbstUI.SDL2RmlUi.Inputs;

/// <summary>
/// Mouse handling using the RmlUi.NET context.
/// </summary>
public class RmlUiMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse
    where TAbstUIMouseEvent : AbstMouseEvent
{
    private readonly Context _context;
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _lingoMouse;
    private int _mouseX;
    private int _mouseY;

    public RmlUiMouse(Context context, Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _context = context;
        _lingoMouse = mouse;
    }

    public void HideMouse(bool state)
    {
        // RmlUi.NET does not expose cursor visibility control yet.
    }

    public void Release()
    {
        // nothing to release yet
    }

    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)lingoMouse);
    }

    public void ProcessMouseMove(int x, int y, KeyModifier modifiers)
    {
        _mouseX = x;
        _mouseY = y;
        _context.ProcessMouseMove(x, y, modifiers);
        _lingoMouse.Value.MouseH = x;
        _lingoMouse.Value.MouseV = y;
        _lingoMouse.Value.DoMouseMove();
    }

    public void ProcessMouseButtonDown(int button, KeyModifier modifiers)
    {
        _context.ProcessMouseButtonDown(button, modifiers);
        _lingoMouse.Value.MouseH = _mouseX;
        _lingoMouse.Value.MouseV = _mouseY;
        if (button == 0)
        {
            _lingoMouse.Value.MouseDown = true;
            _lingoMouse.Value.LeftMouseDown = true;
        }
        else if (button == 1)
        {
            _lingoMouse.Value.RightMouseDown = true;
        }
        else if (button == 2)
        {
            _lingoMouse.Value.MiddleMouseDown = true;
        }
        _lingoMouse.Value.DoMouseDown();
    }

    public void ProcessMouseButtonUp(int button, KeyModifier modifiers)
    {
        _context.ProcessMouseButtonUp(button, modifiers);
        _lingoMouse.Value.MouseH = _mouseX;
        _lingoMouse.Value.MouseV = _mouseY;
        if (button == 0)
        {
            _lingoMouse.Value.MouseDown = false;
            _lingoMouse.Value.LeftMouseDown = false;
        }
        else if (button == 1)
        {
            _lingoMouse.Value.RightMouseDown = false;
        }
        else if (button == 2)
        {
            _lingoMouse.Value.MiddleMouseDown = false;
        }
        _lingoMouse.Value.DoMouseUp();
    }

    public void ProcessMouseWheel(Vector2f delta, KeyModifier modifiers)
    {
        _context.ProcessMouseWheel(delta, modifiers);
        _lingoMouse.Value.DoMouseWheel((int)delta.Y);
    }
}
