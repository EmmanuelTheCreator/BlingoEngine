using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputCheckbox : AbstSdlComponent, IAbstFrameworkInputCheckbox, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        public AbstSdlInputCheckbox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private bool _checked;
        private bool _focused;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
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
                Checked = !Checked;
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
