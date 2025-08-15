using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlSpinBox : AbstSdlComponent, IAbstFrameworkSpinBox, IDisposable
    {
        public AbstSdlSpinBox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
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
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public event Action? ValueChanged;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
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
            return AbstSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
