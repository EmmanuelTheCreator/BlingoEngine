using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Core;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputCheckbox : SdlGfxComponent, ILingoFrameworkGfxInputCheckbox, IDisposable
    {
        public SdlGfxInputCheckbox(SdlFactory factory) : base(factory)
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
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public override nint Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();
            bool val = _checked;
            if (ImGui.Checkbox("##check", ref val))
                Checked = val;
            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return nint.Zero;
        }

        public override void Dispose() => base.Dispose();
    }
}
