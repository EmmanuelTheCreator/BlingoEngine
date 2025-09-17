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
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _blingoMouse;
    private int _mouseX;
    private int _mouseY;

    public RmlUiMouse(Context context, Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _context = context;
        _blingoMouse = mouse;
    }

    public void HideMouse(bool state)
    {
        // RmlUi.NET does not expose cursor visibility control yet.
    }

    public void Release()
    {
        // nothing to release yet
    }

    public void ReplaceMouseObj(IAbstMouse blingoMouse)
    {
        _blingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)blingoMouse);
    }

    public void ProcessMouseMove(int x, int y, KeyModifier modifiers)
    {
        _mouseX = x;
        _mouseY = y;
        _context.ProcessMouseMove(x, y, modifiers);
        _blingoMouse.Value.MouseH = x;
        _blingoMouse.Value.MouseV = y;
        _blingoMouse.Value.DoMouseMove();
    }

    public void ProcessMouseButtonDown(int button, KeyModifier modifiers)
    {
        _context.ProcessMouseButtonDown(button, modifiers);
        _blingoMouse.Value.MouseH = _mouseX;
        _blingoMouse.Value.MouseV = _mouseY;
        if (button == 0)
        {
            _blingoMouse.Value.MouseDown = true;
            _blingoMouse.Value.LeftMouseDown = true;
        }
        else if (button == 1)
        {
            _blingoMouse.Value.RightMouseDown = true;
        }
        else if (button == 2)
        {
            _blingoMouse.Value.MiddleMouseDown = true;
        }
        _blingoMouse.Value.DoMouseDown();
    }

    public void ProcessMouseButtonUp(int button, KeyModifier modifiers)
    {
        _context.ProcessMouseButtonUp(button, modifiers);
        _blingoMouse.Value.MouseH = _mouseX;
        _blingoMouse.Value.MouseV = _mouseY;
        if (button == 0)
        {
            _blingoMouse.Value.MouseDown = false;
            _blingoMouse.Value.LeftMouseDown = false;
        }
        else if (button == 1)
        {
            _blingoMouse.Value.RightMouseDown = false;
        }
        else if (button == 2)
        {
            _blingoMouse.Value.MiddleMouseDown = false;
        }
        _blingoMouse.Value.DoMouseUp();
    }

    public void ProcessMouseWheel(Vector2f delta, KeyModifier modifiers)
    {
        _context.ProcessMouseWheel(delta, modifiers);
        _blingoMouse.Value.DoMouseWheel((int)delta.Y);
    }
}

