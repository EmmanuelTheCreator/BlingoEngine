using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputNumber : ILingoFrameworkGfxInputNumber<float>, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxInputNumber(nint renderer)
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
        public LingoNumberType NumberType { get; set; } = LingoNumberType.Float;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public int FontSize { get; set; } = 12;

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (NumberType == LingoNumberType.Integer)
            {
                int val = (int)_value;
                if (ImGui.InputInt("##num", ref val))
                {
                    val = (int)Math.Clamp(val, (int)Min, (int)Max);
                    Value = val;
                }
            }
            else
            {
                float val = _value;
                if (ImGui.InputFloat("##num", ref val))
                {
                    val = Math.Clamp(val, Min, Max);
                    Value = val;
                }
            }

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
        }

        public void Dispose() { }
    }
}
