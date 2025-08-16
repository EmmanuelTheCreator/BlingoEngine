using System;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorMenu : AbstBlazorComponent, IAbstFrameworkMenu, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstBlazorMenu(AbstBlazorComponentFactory factory, string name) : base(factory)
        {
            Name = name;
        }

        public void AddItem(IAbstFrameworkMenuItem item) { }
        public void ClearItems() { }
        public void PositionPopup(IAbstFrameworkButton button) { }
        public void Popup() { }
        public override void Dispose() => base.Dispose();

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
