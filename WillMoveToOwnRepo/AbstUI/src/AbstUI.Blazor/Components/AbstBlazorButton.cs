using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorButton : AbstBlazorComponent, IAbstFrameworkButton, IDisposable
    {
        public AbstBlazorButton(AbstBlazorComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public IAbstTexture2D? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
