using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputSlider<TValue> : ILingoFrameworkGfxInputSlider<TValue>, IDisposable, ISdlRenderElement where TValue : struct
    {
        private readonly nint _renderer;

        public SdlGfxInputSlider(nint renderer)
        {
            _renderer = renderer;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public bool Enabled { get; set; } = true;
        private TValue _value = default!;
        public TValue Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public TValue MinValue { get; set; } = default!;
        public TValue MaxValue { get; set; } = default!;
        public TValue Step { get; set; } = default!;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public void Render()
        {
            if (!Visibility) return;
            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (typeof(TValue) == typeof(int))
            {
                int v = Convert.ToInt32(_value);
                int min = Convert.ToInt32(MinValue);
                int max = Convert.ToInt32(MaxValue);
                if (ImGui.SliderInt("##slider", ref v, min, max))
                    Value = (TValue)(object)v;
            }
            else
            {
                float v = Convert.ToSingle(_value);
                float min = Convert.ToSingle(MinValue);
                float max = Convert.ToSingle(MaxValue);
                if (ImGui.SliderFloat("##slider", ref v, min, max))
                    Value = (TValue)(object)v;
            }

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
        }

        public void Dispose() { }
    }
}
