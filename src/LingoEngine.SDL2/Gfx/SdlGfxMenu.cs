using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenu : SdlGfxComponent, IAbstUIFrameworkGfxMenu, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public SdlGfxMenu(SdlGfxFactory factory, string name) : base(factory)
        {
            Name = name;
        }

        public void AddItem(IAbstUIFrameworkGfxMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(IAbstUIFrameworkGfxButton button) { }
        public void Popup() { }
        public override void Dispose() => base.Dispose();

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context) => LingoSDLRenderResult.RequireRender();
    }
}
