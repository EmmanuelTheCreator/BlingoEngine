using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputCheckbox : SdlGfxComponent, ILingoFrameworkGfxInputCheckbox, IDisposable
    {
        public SdlGfxInputCheckbox(SdlGfxFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();
            bool val = _checked;
            if (ImGui.Checkbox("##check", ref val))
                Checked = val;
            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return LingoSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
