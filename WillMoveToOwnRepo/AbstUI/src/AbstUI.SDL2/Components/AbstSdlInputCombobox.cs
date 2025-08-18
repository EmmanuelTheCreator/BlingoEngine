using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputCombobox : AbstSdlComponent, IAbstFrameworkInputCombobox, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        public AbstSdlInputCombobox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;
        private bool _focused;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public object FrameworkNode => this;
        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
        }

        public void ClearItems()
        {
            _items.Clear();
            SelectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
        }

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
