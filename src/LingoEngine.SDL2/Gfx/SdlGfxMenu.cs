using System;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenu : SdlGfxComponent, ILingoFrameworkGfxMenu, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;
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

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context) => LingoSDLRenderResult.RequireRender();
    }
}
