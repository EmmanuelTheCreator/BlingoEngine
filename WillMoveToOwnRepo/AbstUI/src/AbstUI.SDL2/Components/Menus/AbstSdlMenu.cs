using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlMenu : AbstSdlComponent, IAbstFrameworkMenu, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstSdlMenu(AbstSdlComponentFactory factory, string name) : base(factory)
        {
            Name = name;
        }

        public void AddItem(IAbstFrameworkMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(IAbstFrameworkButton button) { }
        public void Popup() { }
        public override void Dispose() => base.Dispose();

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context) => AbstSDLRenderResult.RequireRender();
    }
}
