using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework label.
    /// </summary>
    public class AbstUIGfxLabel : AbstUIGfxNodeBase<IAbstUIFrameworkGfxLabel>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public AColor FontColor { get => _framework.FontColor; set => _framework.FontColor = value; }
        public int LineHeight { get => _framework.LineHeight; set => _framework.LineHeight = value; }
        public ATextWrapMode WrapMode { get => _framework.WrapMode; set => _framework.WrapMode = value; }
        public AbstUITextAlignment TextAlignment { get => _framework.TextAlignment; set => _framework.TextAlignment = value; }
    }
}
