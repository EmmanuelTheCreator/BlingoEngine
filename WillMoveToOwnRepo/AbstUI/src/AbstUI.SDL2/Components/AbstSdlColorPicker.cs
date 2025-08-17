using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlColorPicker : AbstSdlComponent, IAbstFrameworkColorPicker, IDisposable
    {
        public AbstSdlColorPicker(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color;
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public override void Dispose() => base.Dispose();
    }
}
