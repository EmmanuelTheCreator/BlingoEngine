using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework label.
    /// </summary>
    public class LingoGfxLabel : LingoGfxNodeBase<ILingoFrameworkGfxLabel>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public LingoColor FontColor { get => _framework.FontColor; set => _framework.FontColor = value; }
        public int LineHeight { get => _framework.LineHeight; set => _framework.LineHeight = value; }
        public LingoTextWrapMode WrapMode { get => _framework.WrapMode; set => _framework.WrapMode = value; }
        public LingoTextAlignment TextAlignment { get => _framework.TextAlignment; set => _framework.TextAlignment = value; }
    }
}
