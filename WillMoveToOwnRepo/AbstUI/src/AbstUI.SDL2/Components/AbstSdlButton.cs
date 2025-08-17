using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlButton : AbstSdlComponent, IAbstFrameworkButton, IDisposable
    {
        public AbstSdlButton(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public IAbstTexture2D? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public void Invoke() => Pressed?.Invoke();
        public override void Dispose() => base.Dispose();
    }
}
