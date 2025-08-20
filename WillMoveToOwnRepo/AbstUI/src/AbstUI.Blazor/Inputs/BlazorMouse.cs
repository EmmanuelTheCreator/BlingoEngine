using AbstUI.Inputs;
using AbstUI.Primitives;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Inputs;

/// <summary>
/// Mouse wrapper using DOM events in Blazor.
/// </summary>
public class BlazorMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _lingoMouse;
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    public BlazorMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse, IJSRuntime js)
    {
        _lingoMouse = mouse;
        _js = js;
    }

    public void SetMouse(AbstMouse<TAbstUIMouseEvent> mouse)
        => _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => mouse);

    public void HideMouse(bool state)
        => _ = SetCursorCss(state ? "none" : "default");

    public virtual void Release() { }
    public void SetOffset(int x, int y)
    {
        OffsetX = x;
        OffsetY = y;
    }
    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)lingoMouse);
    }

    public void MouseMove(MouseEventArgs e)
    {
        _lingoMouse.Value.MouseH = (int)e.ClientX;
        _lingoMouse.Value.MouseV = (int)e.ClientY;
        _lingoMouse.Value.DoMouseMove();
    }

    public void MouseDown(MouseEventArgs e)
    {
        _lingoMouse.Value.MouseH = (int)e.ClientX;
        _lingoMouse.Value.MouseV = (int)e.ClientY;
        if (e.Button == 0)
        {
            _lingoMouse.Value.MouseDown = true;
            _lingoMouse.Value.LeftMouseDown = true;
            _lingoMouse.Value.DoubleClick = e.Detail > 1;
        }
        else if (e.Button == 2)
        {
            _lingoMouse.Value.RightMouseDown = true;
        }
        else if (e.Button == 1)
        {
            _lingoMouse.Value.MiddleMouseDown = true;
        }
        _lingoMouse.Value.DoMouseDown();
    }

    public void MouseUp(MouseEventArgs e)
    {
        _lingoMouse.Value.MouseH = (int)e.ClientX;
        _lingoMouse.Value.MouseV = (int)e.ClientY;
        if (e.Button == 0)
        {
            _lingoMouse.Value.MouseDown = false;
            _lingoMouse.Value.LeftMouseDown = false;
        }
        else if (e.Button == 2)
        {
            _lingoMouse.Value.RightMouseDown = false;
        }
        else if (e.Button == 1)
        {
            _lingoMouse.Value.MiddleMouseDown = false;
        }
        _lingoMouse.Value.DoMouseUp();
    }

    public void Wheel(WheelEventArgs e)
    {
        _lingoMouse.Value.MouseH = (int)e.ClientX;
        _lingoMouse.Value.MouseV = (int)e.ClientY;
        _lingoMouse.Value.DoMouseWheel((int)e.DeltaY);
    }

    public void SetCursor(AMouseCursor value)
    {
        var cursor = value switch
        {
            AMouseCursor.Cross => "crosshair",
            AMouseCursor.Watch => "wait",
            AMouseCursor.IBeam => "text",
            AMouseCursor.SizeAll => "move",
            AMouseCursor.SizeNWSE => "nwse-resize",
            AMouseCursor.SizeNESW => "nesw-resize",
            AMouseCursor.SizeWE => "ew-resize",
            AMouseCursor.SizeNS => "ns-resize",
            AMouseCursor.UpArrow => "default",
            AMouseCursor.Blank => "default",
            AMouseCursor.Finger => "pointer",
            AMouseCursor.Drag => "move",
            AMouseCursor.Help => "help",
            AMouseCursor.Wait => "wait",
            AMouseCursor.NotAllowed => "not-allowed",
            _ => "default"
        };
        _ = SetCursorCss(cursor);
    }

    private async Task SetCursorCss(string cursor)
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js");
        await _module.InvokeVoidAsync("AbstUIKey.setCursor", cursor);
    }
}
