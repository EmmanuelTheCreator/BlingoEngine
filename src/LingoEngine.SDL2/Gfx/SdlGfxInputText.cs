using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputText : SdlGfxComponent, ILingoFrameworkGfxInputText, IDisposable
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
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public object FrameworkNode => this;

        public event Action? ValueChanged;

        public override nint Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            uint len = MaxLength > 0 ? (uint)MaxLength : 1024u;
            if (ImGui.InputText("##text", ref _text, len))
                ValueChanged?.Invoke();

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return nint.Zero;
        }

        public override void Dispose() => base.Dispose();
    }
}
