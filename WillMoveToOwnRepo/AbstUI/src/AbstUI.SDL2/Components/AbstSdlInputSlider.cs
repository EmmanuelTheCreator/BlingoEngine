using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputSlider<TValue> : AbstSdlComponent, IAbstFrameworkInputSlider<TValue>, IDisposable where TValue : struct
    {
        public AbstSdlInputSlider(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
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
            return AbstSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
