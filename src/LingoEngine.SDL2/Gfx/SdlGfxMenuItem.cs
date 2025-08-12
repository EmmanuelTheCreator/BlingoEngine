using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenuItem : SdlGfxComponent, ILingoFrameworkGfxMenuItem, IDisposable
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

        public override nint Render(LingoSDLRenderContext context) => nint.Zero;
    }
}
