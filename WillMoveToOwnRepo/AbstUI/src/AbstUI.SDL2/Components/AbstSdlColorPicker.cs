using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlColorPicker : AbstSdlComponent, IAbstFrameworkColorPicker, ISdlFocusable, IDisposable
    {
        public AbstSdlColorPicker(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color;
        private bool _focused;
        public AColor Color
        {
            get => _color;
            set
            {
                if (!_color.Equals(value))
                {
                    _color = value;
                    ValueChanged?.Invoke();
                }
            }
        }

        public event Action? ValueChanged;
        public object FrameworkNode => this;

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
