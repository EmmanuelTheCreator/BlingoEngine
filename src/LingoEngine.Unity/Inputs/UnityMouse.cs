using LingoEngine.Bitmaps;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using System;
using UnityEngine;

namespace LingoEngine.Unity.Inputs;

public class UnityMouse : ILingoFrameworkMouse
{
    private Lazy<LingoMouse> _lingoMouse;
    private bool _hidden;
    private LingoMemberBitmap? _cursorImage;
    private LingoMouseCursor _cursor = LingoMouseCursor.Arrow;

    public UnityMouse(Lazy<LingoMouse> mouse)
    {
        _lingoMouse = mouse;
    }

    internal void SetLingoMouse(LingoMouse mouse) => _lingoMouse = new Lazy<LingoMouse>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
    }

    public void SetCursor(LingoMemberBitmap image)
    {
        _cursorImage = image;
        var bmp = image.Framework<Unity.Bitmaps.UnityMemberBitmap>();
        bmp.Preload();
        var tex = bmp.TextureUnity;
        if (tex != null)
            Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
    }

    public void SetCursor(LingoMouseCursor value)
    {
        _cursor = value;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Release()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ReplaceMouseObj(LingoMouse lingoMouse) => _lingoMouse = new Lazy<LingoMouse>(() => lingoMouse);

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
