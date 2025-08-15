using System;
using AbstUI.Components;
using AbstUI.SDL2;

namespace AbstUI.SDL2.Components
{
    internal class SdlGfxMenuItem : SdlGfxComponent, IAbstUIFrameworkGfxMenuItem, IDisposable
    {
        public bool Enabled { get; set; } = true;
        public bool CheckMark { get; set; }
        public string? Shortcut { get; set; }
        public event Action? Activated;
        public object FrameworkNode => this;

        public SdlGfxMenuItem(SdlGfxFactory factory, string name, string? shortcut) : base(factory)
        {
            Name = name;
            Shortcut = shortcut;
        }

        public void Invoke() => Activated?.Invoke();
        public override void Dispose() => base.Dispose();

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context) => LingoSDLRenderResult.RequireRender();
    }
}
