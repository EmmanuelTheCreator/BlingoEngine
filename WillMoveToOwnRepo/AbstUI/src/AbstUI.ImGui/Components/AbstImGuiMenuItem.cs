using System;
using AbstUI.Components;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiMenuItem : AbstImGuiComponent, IAbstFrameworkMenuItem, IDisposable
    {
        public bool Enabled { get; set; } = true;
        public bool CheckMark { get; set; }
        public string? Shortcut { get; set; }
        public event Action? Activated;
        public object FrameworkNode => this;

        public AbstImGuiMenuItem(AbstImGuiComponentFactory factory, string name, string? shortcut) : base(factory)
        {
            Name = name;
            Shortcut = shortcut;
        }

        public void Invoke() => Activated?.Invoke();
        public override void Dispose() => base.Dispose();

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context) => AbstImGuiRenderResult.RequireRender();
    }
}
