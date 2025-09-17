using AbstUI.Inputs;
using AbstUI.Primitives;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Inputs;

/// <summary>
/// Mouse wrapper using DOM events in Blazor.
/// </summary>
public class BlazorMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<AbstMouse<TAbstUIMouseEvent>> where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _blingoMouse;
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;
    private AMouseCursor _lastCursor = AMouseCursor.Arrow;

    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    public BlazorMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse, IJSRuntime js)
    {
        _blingoMouse = mouse;
        _js = js;
    }

    public void SetMouse(AbstMouse<TAbstUIMouseEvent> mouse)
        => _blingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => mouse);

    public void HideMouse(bool state)
        => _ = SetCursorCss(state ? "none" : "default");

    public virtual void Release() { }
    public void SetOffset(int x, int y)
    {
        OffsetX = x;
        OffsetY = y;
    }
    public void ReplaceMouseObj(IAbstMouse blingoMouse)
    {
        _blingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)blingoMouse);
    }

    public void MouseMove(MouseEventArgs e)
    {
        _blingoMouse.Value.MouseH = (int)e.ClientX;
        _blingoMouse.Value.MouseV = (int)e.ClientY;
        _blingoMouse.Value.DoMouseMove();
    }

    public void MouseDown(MouseEventArgs e)
    {
        _blingoMouse.Value.MouseH = (int)e.ClientX;
        _blingoMouse.Value.MouseV = (int)e.ClientY;
        if (e.Button == 0)
        {
            _blingoMouse.Value.MouseDown = true;
            _blingoMouse.Value.LeftMouseDown = true;
            _blingoMouse.Value.DoubleClick = e.Detail > 1;
        }
        else if (e.Button == 2)
        {
            _blingoMouse.Value.RightMouseDown = true;
        }
        else if (e.Button == 1)
        {
            _blingoMouse.Value.MiddleMouseDown = true;
        }
        _blingoMouse.Value.DoMouseDown();
    }

    public void MouseUp(MouseEventArgs e)
    {
        _blingoMouse.Value.MouseH = (int)e.ClientX;
        _blingoMouse.Value.MouseV = (int)e.ClientY;
        if (e.Button == 0)
        {
            _blingoMouse.Value.MouseDown = false;
            _blingoMouse.Value.LeftMouseDown = false;
        }
        else if (e.Button == 2)
        {
            _blingoMouse.Value.RightMouseDown = false;
        }
        else if (e.Button == 1)
        {
            _blingoMouse.Value.MiddleMouseDown = false;
        }
        _blingoMouse.Value.DoMouseUp();
    }

    public void Wheel(WheelEventArgs e)
    {
        _blingoMouse.Value.MouseH = (int)e.ClientX;
        _blingoMouse.Value.MouseV = (int)e.ClientY;
        _blingoMouse.Value.DoMouseWheel((int)e.DeltaY);
    }

    public void SetCursor(AMouseCursor value)
    {
        _lastCursor = value;
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
    public AMouseCursor GetCursor() => _lastCursor;

    private async Task SetCursorCss(string cursor)
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js");
        await _module.InvokeVoidAsync("AbstUIKey.setCursor", cursor);
    }

   
}

