using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorInputText : AbstBlazorComponent, IAbstFrameworkInputText, IDisposable
    {
        public AbstBlazorInputText(AbstBlazorComponentFactory factory, bool multiLine) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public int MaxLength { get; set; }
        public string? Font { get; set; }
        public int FontSize { get; set; } = 12;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public bool IsMultiLine { get; set; }

        public event Action? ValueChanged;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
