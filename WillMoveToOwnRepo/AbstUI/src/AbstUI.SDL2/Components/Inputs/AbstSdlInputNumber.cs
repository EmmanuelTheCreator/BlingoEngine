using System;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Inputs;

internal class AbstSdlInputNumber<TValue> : AbstSdlComponent, IAbstFrameworkInputNumber<TValue>, IFrameworkFor<AbstInputNumber<TValue>>, IHandleSdlEvent, ISdlFocusable, IDisposable, IHasTextBackgroundBorderColor
#if NET48
    where TValue : struct
#else
    where TValue : INumber<TValue>
#endif
{
    private readonly AbstSdlInputText _textInput;



    public bool Enabled { get; set; } = true;

    private TValue _value = TValue.Zero;
    public TValue Value
    {
        get => _value;
        set
        {
            var v = Clamp(value);
            if (!_value.Equals(v))
            {
                _value = v;
                _textInput.Text = v.ToString();
                ValueChanged?.Invoke();
            }
        }
    }

    public TValue Min { get; set; }
    public TValue Max { get; set; }

    public TValue Step { get; set; } = TValue.One;

    public ANumberType NumberType { get; set; } = ANumberType.Float;
    public AMargin Margin { get; set; } = AMargin.Zero;
    public event Action? ValueChanged;
    public object FrameworkNode => this;

    public int FontSize
    {
        get => _textInput.FontSize;
        set => _textInput.FontSize = value;
    }

    public string? Font
    {
        get => _textInput.Font;
        set => _textInput.Font = value;
    }

    public AColor TextColor
    {
        get => _textInput.TextColor;
        set => _textInput.TextColor = value;
    }

    public AColor BackgroundColor
    {
        get => _textInput.BackgroundColor;
        set => _textInput.BackgroundColor = value;
    }

    public AColor BorderColor
    {
        get => _textInput.BorderColor;
        set => _textInput.BorderColor = value;
    }

    private TValue Clamp(TValue v)
    {
        if (v < Min) v = Min;
        if (v > Max) v = Max;
        return v;
    }
    public AbstSdlInputNumber(AbstSdlComponentFactory factory) : base(factory)
    {
        _textInput = new AbstSdlInputText(factory, false);
        _textInput.ValueChanged += OnTextChanged;
        Width = 50;
        Height = 20;
    }
    private void OnTextChanged()
    {
        if (TValue.TryParse(_textInput.Text, null, out var v))
        {
            v = Clamp(v);
            if (!_value.Equals(v))
            {
                _value = v;
                ValueChanged?.Invoke();
            }
        }
    }
    
    public void HandleEvent(AbstSDLEvent e)
    {
        if (!Enabled) return;

        _textInput.HandleEvent(e);
        if (e.StopPropagation) return;

        ref var ev = ref e.Event;

        if (ev.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
        {
            if (ev.wheel.y > 0) Value += Step * TValue.CreateChecked(ev.wheel.y);
            else if (ev.wheel.y < 0) Value -= Step * TValue.CreateChecked(-ev.wheel.y);
            e.StopPropagation = true;
            return;
        }

        if (ev.type == SDL.SDL_EventType.SDL_KEYDOWN && _textInput.HasFocus)
        {
            if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
            {
                Value += Step;
                e.StopPropagation = true;
                return;
            }
            if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
            {
                Value -= Step;
                e.StopPropagation = true;
                return;
            }
        }
    }


    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return default;
        _textInput.X = X;
        _textInput.Y = Y;
        _textInput.Width = Width;
        _textInput.Height = Height;
        return _textInput.Render(context);
    }

    public override void Dispose()
    {
        base.Dispose();
        _textInput.Dispose();
    }

    public bool HasFocus => _textInput.HasFocus;

    public void SetFocus(bool focus) => _textInput.SetFocus(focus);
}

