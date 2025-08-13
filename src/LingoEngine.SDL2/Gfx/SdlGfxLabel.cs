using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxLabel : SdlGfxComponent, ILingoFrameworkGfxLabel, IDisposable
    {
        public SdlGfxLabel(SdlGfxFactory factory) : base(factory)
        {
        }
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

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility || string.IsNullOrEmpty(Text))
                return default; // DoRender=false

            var vpPos = context.ImGuiViewPort.WorkPos;
            var basePos = vpPos + new Vector2(X, Y);

            ImGui.PushID(Name);
            ImGui.PushStyleColor(ImGuiCol.Text, FontColor.ToImGuiColor());

            // Wrapping (only when Width > 0)
            bool wrap = WrapMode != LingoTextWrapMode.Off && Width > 0;
            if (wrap) ImGui.PushTextWrapPos(basePos.X + Width);

            // Position with screen coords (no assertions)
            if (!wrap && Width > 0 && TextAlignment != LingoTextAlignment.Left)
            {
                // single-line alignment
                var sz = ImGui.CalcTextSize(Text);
                float startX = basePos.X;
                if (TextAlignment == LingoTextAlignment.Center) startX += (Width - sz.X) / 2f;
                else if (TextAlignment == LingoTextAlignment.Right) startX += (Width - sz.X);
                ImGui.SetCursorScreenPos(new Vector2(startX, basePos.Y));
                ImGui.TextUnformatted(Text);
            }
            else
            {
                // left aligned (or wrapped)
                ImGui.SetCursorScreenPos(basePos);
                if (wrap) ImGui.TextWrapped(Text);
                else ImGui.TextUnformatted(Text);
            }

            if (wrap) ImGui.PopTextWrapPos();
            ImGui.PopStyleColor();
            ImGui.PopID();

            return LingoSDLRenderResult.RequireRender(); // UI drawn via ImGui, no SDL texture
        }


        public override void Dispose() => base.Dispose();
    }
}
