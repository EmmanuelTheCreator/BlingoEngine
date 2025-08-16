using System;
using System.Numerics;
using AbstUI.Texts;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiLabel : AbstImGuiComponent, IAbstFrameworkLabel, IDisposable
    {
        public AbstImGuiLabel(AbstImGuiComponentFactory factory) : base(factory)
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

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility || string.IsNullOrEmpty(Text))
                return default; // DoRender=false
            ImFontPtr? font = null;
            if (FontSize > 0)
            {
                //font = context.GetFont(FontSize);
                //if (font.HasValue)
                //    global::ImGuiNET.ImGui.PushFont(font.Value);
            }
            var basePos = context.Origin + new Vector2(X, Y);
            global::ImGuiNET.ImGui.SetCursorScreenPos(basePos);
            global::ImGuiNET.ImGui.PushID(Name);
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Text, FontColor.ToImGuiColor());


            // Wrapping (only when Width > 0)
            bool wrap = WrapMode != ATextWrapMode.Off && Width > 0;
            if (wrap) global::ImGuiNET.ImGui.PushTextWrapPos(basePos.X + Width);

            // Position with screen coords (no assertions)
            if (!wrap && Width > 0 && TextAlignment != AbstTextAlignment.Left)
            {
                // single-line alignment
                var sz = global::ImGuiNET.ImGui.CalcTextSize(Text);
                float startX = basePos.X;
                if (TextAlignment == AbstTextAlignment.Center) startX += (Width - sz.X) / 2f;
                else if (TextAlignment == AbstTextAlignment.Right) startX += Width - sz.X;
                global::ImGuiNET.ImGui.SetCursorScreenPos(new Vector2(startX, basePos.Y));
                global::ImGuiNET.ImGui.TextUnformatted(Text);
            }
            else
            {
                // left aligned (or wrapped)
                global::ImGuiNET.ImGui.SetCursorScreenPos(basePos);
                if (wrap) global::ImGuiNET.ImGui.TextWrapped(Text);
                else global::ImGuiNET.ImGui.TextUnformatted(Text);
            }

            if (wrap) global::ImGuiNET.ImGui.PopTextWrapPos();
            global::ImGuiNET.ImGui.PopStyleColor();
            global::ImGuiNET.ImGui.PopID();
            if (font.HasValue)
                global::ImGuiNET.ImGui.PopFont();
            return AbstImGuiRenderResult.RequireRender(); // UI drawn via ImGui, no SDL texture
        }


        public override void Dispose() => base.Dispose();
    }
}
