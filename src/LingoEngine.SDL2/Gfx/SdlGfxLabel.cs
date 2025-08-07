using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxLabel : ILingoFrameworkGfxLabel, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxLabel(nint renderer)
        {
            _renderer = renderer;
        }
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

        public LingoColor FontColor { get; set; } = LingoColorList.Black;
        private int _lineHeight;
        public int LineHeight { get => _lineHeight; set => _lineHeight = value; }
        private LingoTextWrapMode _wrapMode;
        public LingoTextWrapMode WrapMode { get => _wrapMode; set => _wrapMode = value; }
        private LingoTextAlignment _textAlignment;
        public LingoTextAlignment TextAlignment { get => _textAlignment; set => _textAlignment = value; }

        public object FrameworkNode => this;


        public event Action? ValueChanged;

        public void Render()
        {
            if (!Visibility) return;

            ImGui.PushID(Name);
            var pos = new Vector2(X, Y);
            ImGui.SetCursorPos(pos);

            ImGui.PushStyleColor(ImGuiCol.Text, FontColor.ToImGuiColor());
            if (WrapMode != LingoTextWrapMode.Off)
                ImGui.PushTextWrapPos(X + Width);

            if (TextAlignment != LingoTextAlignment.Left)
            {
                float wrapWidth = WrapMode == LingoTextWrapMode.Off ? 0 : Width;
                Vector2 size = ImGui.CalcTextSize(Text, wrapWidth);
                float startX = pos.X;
                if (TextAlignment == LingoTextAlignment.Center)
                    startX += (Width - size.X) / 2f;
                else if (TextAlignment == LingoTextAlignment.Right)
                    startX += (Width - size.X);
                ImGui.SetCursorPos(new Vector2(startX, pos.Y));
            }

            ImGui.TextUnformatted(Text);

            if (WrapMode != LingoTextWrapMode.Off)
                ImGui.PopTextWrapPos();
            ImGui.PopStyleColor();
            ImGui.PopID();
        }

        public void Dispose() { }
    }
}
