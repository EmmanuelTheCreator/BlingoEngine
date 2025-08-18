using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputSlider<TValue> : AbstSdlComponent, IAbstFrameworkInputSlider<TValue>, IHandleSdlEvent, ISdlFocusable, IDisposable where TValue : struct
    {
        public AbstSdlInputSlider(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public bool Enabled { get; set; } = true;
        private TValue _value = default!;
        private bool _focused;
        public TValue Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public TValue MinValue { get; set; } = default!;
        public TValue MaxValue { get; set; } = default!;
        public TValue Step { get; set; } = default!;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT &&
                HitTest(ev.button.x, ev.button.y))
            {
                Factory.FocusManager.SetFocus(this);
                e.StopPropagation = true;
            }
        }

        private bool HitTest(int x, int y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public override void Dispose() => base.Dispose();
    }
}
