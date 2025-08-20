using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;

namespace AbstUI.SDL2.Components.Inputs;

internal class AbstSdlSpinBox : AbstSdlComponent, IAbstFrameworkSpinBox, IHandleSdlEvent, ISdlFocusable, IDisposable
{
    private readonly AbstSdlInputNumber<float> _number;
    private const int ButtonWidth = 16;

    public AMargin Margin { get; set; } = AMargin.Zero;
    public object FrameworkNode => this;
    public event Action? ValueChanged;
    public bool Enabled { get; set; } = true;

    public float Value
    {
        get => _number.Value;
        set => _number.Value = value;
    }

    public float Min
    {
        get => _number.Min;
        set => _number.Min = value;
    }

    public float Max
    {
        get => _number.Max;
        set => _number.Max = value;
    }

    public float Step
    {
        get => _number.Step;
        set => _number.Step = value;
    }

  
    public AbstSdlSpinBox(AbstSdlComponentFactory factory) : base(factory)
    {
        _number = new AbstSdlInputNumber<float>(factory);
        _number.ValueChanged += () => ValueChanged?.Invoke();
        Width = 50;
        Height = 20;
    }

    public void HandleEvent(AbstSDLEvent e)
    {
        if (!Enabled) return;

        SDL.SDL_GetMouseState(out var mx, out var my);

        ref var ev = ref e.Event;

        if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
        {
            var upRect = GetUpRect();
            var downRect = GetDownRect();
            if (PointInRect(mx, my, upRect))
            {
                Value += Step;
                e.StopPropagation = true;
                return;
            }
            if (PointInRect(mx, my, downRect))
            {
                Value -= Step;
                e.StopPropagation = true;
                return;
            }
        }

        _number.HandleEvent(e);
    }

    private SDL.SDL_Rect GetUpRect() => new SDL.SDL_Rect
    {
        x = (int)(X + Width - ButtonWidth),
        y = (int)Y,
        w = ButtonWidth,
        h = (int)Height / 2
    };

    private SDL.SDL_Rect GetDownRect() => new SDL.SDL_Rect
    {
        x = (int)(X + Width - ButtonWidth),
        y = (int)Y + (int)Height / 2,
        w = ButtonWidth,
        h = (int)Height / 2
    };

    private static bool PointInRect(int x, int y, SDL.SDL_Rect r)
        => x >= r.x && x < r.x + r.w && y >= r.y && y < r.y + r.h;

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return default;

        _number.X = X;
        _number.Y = Y;
        _number.Width = Width - ButtonWidth;
        _number.Height = Height;
        _number.Render(context);

        var renderer = context.Renderer;
        var up = GetUpRect();
        var down = GetDownRect();
        var accent = AbstDefaultColors.InputAccentColor;
        SDL.SDL_SetRenderDrawColor(renderer, accent.R, accent.G, accent.B, accent.A);
        SDL.SDL_RenderFillRect(renderer, ref up);
        SDL.SDL_RenderFillRect(renderer, ref down);
        var border = AbstDefaultColors.InputBorderColor;
        SDL.SDL_SetRenderDrawColor(renderer, border.R, border.G, border.B, border.A);
        SDL.SDL_RenderDrawRect(renderer, ref up);
        SDL.SDL_RenderDrawRect(renderer, ref down);
        // simple triangles
        SDL.SDL_RenderDrawLine(renderer, up.x + up.w / 2, up.y + 3, up.x + 3, up.y + up.h - 3);
        SDL.SDL_RenderDrawLine(renderer, up.x + up.w / 2, up.y + 3, up.x + up.w - 3, up.y + up.h - 3);
        SDL.SDL_RenderDrawLine(renderer, down.x + 3, down.y + 3, down.x + down.w / 2, down.y + down.h - 3);
        SDL.SDL_RenderDrawLine(renderer, down.x + down.w - 3, down.y + 3, down.x + down.w / 2, down.y + down.h - 3);

        return AbstSDLRenderResult.RequireRender();
    }

    public override void Dispose()
    {
        base.Dispose();
        _number.Dispose();
    }

    public bool HasFocus => _number.HasFocus;

    public void SetFocus(bool focus) => _number.SetFocus(focus);
}

