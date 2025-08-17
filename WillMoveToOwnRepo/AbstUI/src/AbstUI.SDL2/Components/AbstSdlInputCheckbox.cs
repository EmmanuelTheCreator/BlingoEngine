using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputCheckbox : AbstSdlComponent, IAbstFrameworkInputCheckbox, IDisposable
    {
        public AbstSdlInputCheckbox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private bool _checked;
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public override void Dispose() => base.Dispose();
    }
}
