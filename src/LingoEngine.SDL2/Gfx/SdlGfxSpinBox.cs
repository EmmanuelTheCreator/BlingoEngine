using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxSpinBox : ILingoFrameworkGfxSpinBox, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxSpinBox(nint renderer)
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
        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public float Min { get; set; }
        public float Max { get; set; }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public object FrameworkNode => this;

        public event Action? ValueChanged;

        public void Render()
        {
            if (!Visibility) return;
            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();
            float val = _value;
            if (ImGui.InputFloat("##spin", ref val, 1f))
            {
                val = Math.Clamp(val, Min, Max);
                Value = val;
            }
            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
        }

        public void Dispose() { }
    }
}
