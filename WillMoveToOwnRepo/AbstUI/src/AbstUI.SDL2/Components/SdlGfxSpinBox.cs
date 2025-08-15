using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;

namespace AbstUI.SDL2.Components
{
    internal class SdlGfxSpinBox : SdlGfxComponent, IAbstUIFrameworkGfxSpinBox, IDisposable
    {
        public SdlGfxSpinBox(SdlGfxFactory factory) : base(factory)
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

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
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
            return LingoSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
