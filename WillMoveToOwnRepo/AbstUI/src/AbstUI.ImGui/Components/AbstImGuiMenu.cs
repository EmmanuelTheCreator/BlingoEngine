using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiMenu : AbstImGuiComponent, IAbstFrameworkMenu, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstImGuiMenu(AbstImGuiComponentFactory factory, string name) : base(factory)
        {
            Name = name;
        }

        public void AddItem(IAbstFrameworkMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(IAbstFrameworkButton button) { }
        public void Popup() { }
        public override void Dispose() => base.Dispose();

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context) => AbstImGuiRenderResult.RequireRender();
    }
}
