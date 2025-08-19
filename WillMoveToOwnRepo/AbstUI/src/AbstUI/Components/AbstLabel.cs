using AbstUI.Primitives;
using AbstUI.Texts;
using AbstUI.Styles;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework label.
    /// </summary>
    public class AbstLabel : AbstNodeBase<IAbstFrameworkLabel>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public AColor FontColor { get => _framework.FontColor; set => _framework.FontColor = value; }
        public int LineHeight { get => _framework.LineHeight; set => _framework.LineHeight = value; }
        public ATextWrapMode WrapMode { get => _framework.WrapMode; set => _framework.WrapMode = value; }
        public AbstTextAlignment TextAlignment { get => _framework.TextAlignment; set => _framework.TextAlignment = value; }

        protected override void OnSetStyle(AbstComponentStyle componentStyle)
        {
            base.OnSetStyle(componentStyle);
            if (componentStyle is AbstLabelStyle style)
            {
                if (style.Font != null) Font = style.Font;
                if (style.FontSize.HasValue) FontSize = style.FontSize.Value;
                if (style.FontColor.HasValue) FontColor = style.FontColor.Value;
            }
        }

        protected override void OnGetStyle(AbstComponentStyle componentStyle)
        {
            base.OnGetStyle(componentStyle);
            if (componentStyle is AbstLabelStyle style)
            {
                style.Font = Font;
                style.FontSize = FontSize;
                style.FontColor = FontColor;
            }
        }
    }
}
