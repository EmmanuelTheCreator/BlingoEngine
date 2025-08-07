using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxLabel : ILingoFrameworkGfxLabel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        public string Text { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public string? Font { get; set; }
        public LingoColor FontColor { get; set; }
        public int LineHeight { get; set; }

        public object FrameworkNode => this;

        public LingoTextWrapMode WrapMode { get; set; }
        public LingoTextAlignment TextAlignment { get; set; }

        public event Action? ValueChanged;
        public void Dispose() { }
    }
}
