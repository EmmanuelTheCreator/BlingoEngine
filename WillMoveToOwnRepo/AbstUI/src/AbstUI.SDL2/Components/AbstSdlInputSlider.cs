using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputSlider<TValue> : AbstSdlComponent, IAbstFrameworkInputSlider<TValue>, IDisposable where TValue : struct
    {
        public AbstSdlInputSlider(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public bool Enabled { get; set; } = true;
        private TValue _value = default!;
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public override void Dispose() => base.Dispose();
    }
}
