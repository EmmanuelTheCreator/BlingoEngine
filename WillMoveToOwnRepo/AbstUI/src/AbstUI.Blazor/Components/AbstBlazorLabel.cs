using System;
using System.Numerics;
using AbstUI.Texts;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace AbstUI.Blazor.Components
{
    {
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;

        public string Text { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public string? Font { get; set; }

        public AColor FontColor { get; set; } = AColors.Black;
        private int _lineHeight;
        public int LineHeight { get => _lineHeight; set => _lineHeight = value; }
        private ATextWrapMode _wrapMode;
        public ATextWrapMode WrapMode { get => _wrapMode; set => _wrapMode = value; }
        private AbstTextAlignment _textAlignment;
        public AbstTextAlignment TextAlignment { get => _textAlignment; set => _textAlignment = value; }

        public object FrameworkNode => this;


        public event Action? ValueChanged;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
