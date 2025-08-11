using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputCheckbox : ILingoFrameworkGfxInputCheckbox, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxInputCheckbox(nint renderer)
        {
            _renderer = renderer;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
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

        public void Render()
        {
            if (!Visibility) return;
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
        }

        public void Dispose() { }
    }
}
