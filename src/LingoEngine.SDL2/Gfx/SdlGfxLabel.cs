using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;
using LingoEngine.SDL2.Primitives;
using LingoEngine.Texts;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxLabel : SdlGfxComponent, ILingoFrameworkGfxLabel, IDisposable
    {
        public SdlGfxLabel(SdlGfxFactory factory) : base(factory)
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
        private LingoTextAlignment _textAlignment;
        public LingoTextAlignment TextAlignment { get => _textAlignment; set => _textAlignment = value; }

        public object FrameworkNode => this;


        public event Action? ValueChanged;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility || string.IsNullOrEmpty(Text))
                return default; // DoRender=false
            ImFontPtr? font = null;
            if (FontSize > 0)
            {
                //font = context.GetFont(FontSize);
                //if (font.HasValue)
                //    ImGui.PushFont(font.Value);
            }
            var basePos = context.Origin + new Vector2(X, Y);
            ImGui.SetCursorScreenPos(basePos);
            ImGui.PushID(Name);
            ImGui.PushStyleColor(ImGuiCol.Text, FontColor.ToImGuiColor());
            

            // Wrapping (only when Width > 0)
            bool wrap = WrapMode != ATextWrapMode.Off && Width > 0;
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
            if (font.HasValue)
                ImGui.PopFont();
            return LingoSDLRenderResult.RequireRender(); // UI drawn via ImGui, no SDL texture
        }


        public override void Dispose() => base.Dispose();
    }
}
