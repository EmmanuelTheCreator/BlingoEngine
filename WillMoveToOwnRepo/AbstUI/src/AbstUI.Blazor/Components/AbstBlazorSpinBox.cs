using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorSpinBox : AbstBlazorComponent, IAbstFrameworkSpinBox, IDisposable
    {
        public AbstBlazorSpinBox(AbstBlazorComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public float Min { get; set; }
        public float Max { get; set; }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public event Action? ValueChanged;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
