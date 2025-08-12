using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenu : SdlGfxComponent, ILingoFrameworkGfxMenu, IDisposable
    {
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public object FrameworkNode => this;

        public SdlGfxMenu(SdlGfxFactory factory, string name) : base(factory)
        {
            Name = name;
        }

        public void AddItem(ILingoFrameworkGfxMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(ILingoFrameworkGfxButton button) { }
        public void Popup() { }
        public override void Dispose() => base.Dispose();

        public override nint Render(LingoSDLRenderContext context) => nint.Zero;
    }
}
