using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;

namespace AbstUI.SDL2.Components
{
    internal class SdlGfxInputText : SdlGfxComponent, IAbstUIFrameworkGfxInputText, IDisposable
    {
        public SdlGfxInputText(SdlGfxFactory factory) : base(factory)
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

        public event Action? ValueChanged;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            // place relative to current context origin
            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));

            if (Width > 0) ImGui.SetNextItemWidth(Width);

            ImGui.PushID(Name);
            if (!Enabled) ImGui.BeginDisabled();

            uint cap = MaxLength > 0 ? (uint)MaxLength : 1024u;
            if (ImGui.InputText("##text", ref _text, cap))
                ValueChanged?.Invoke();

            if (!Enabled) ImGui.EndDisabled();
            ImGui.PopID();
            return LingoSDLRenderResult.RequireRender();
        }


        public override void Dispose() => base.Dispose();
    }
}
