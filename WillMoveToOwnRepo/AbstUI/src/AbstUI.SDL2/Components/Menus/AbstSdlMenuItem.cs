using System;
using AbstUI.Components.Menus;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Menus
{
    internal class AbstSdlMenuItem : AbstSdlComponent, IAbstFrameworkMenuItem, IDisposable
    {
        public bool Enabled { get; set; } = true;
        public bool CheckMark { get; set; }
        public string? Shortcut { get; set; }
        public event Action? Activated;
        public object FrameworkNode => this;

        public AbstSdlMenuItem(AbstSdlComponentFactory factory, string name, string? shortcut) : base(factory)
        {
            Name = name;
            Shortcut = shortcut;
            Width = 80;
            Height = 20;
        }

        public void Invoke() => Activated?.Invoke();
        public override void Dispose() => base.Dispose();

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context) => AbstSDLRenderResult.RequireRender();
    }
}
