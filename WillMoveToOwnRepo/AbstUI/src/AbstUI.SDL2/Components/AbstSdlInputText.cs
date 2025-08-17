using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputText : AbstSdlComponent, IAbstFrameworkInputText, IDisposable
    {
        public AbstSdlInputText(AbstSdlComponentFactory factory, bool multiLine) : base(factory)
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }


        public override void Dispose() => base.Dispose();
    }
}
