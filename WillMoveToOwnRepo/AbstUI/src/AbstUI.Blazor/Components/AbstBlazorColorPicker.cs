using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorColorPicker : AbstBlazorComponent, IAbstFrameworkColorPicker, IDisposable
    {
        public AbstBlazorColorPicker(AbstBlazorComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color;
        public AColor Color
        {
            get => _color;
            set
            {
                if (!_color.Equals(value))
                {
                    _color = value;
                    ValueChanged?.Invoke();
                }
            }
        }

        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
