using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorInputNumber : AbstBlazorComponent, IAbstFrameworkInputNumber<float>, IDisposable
    {
        public AbstBlazorInputNumber(AbstBlazorComponentFactory factory) : base(factory)
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
        public ANumberType NumberType { get; set; } = ANumberType.Float;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public int FontSize { get; set; } = 12;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
