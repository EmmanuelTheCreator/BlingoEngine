using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputNumber : SdlGfxComponent, IAbstUIFrameworkGfxInputNumber<float>, IDisposable
    {
        public SdlGfxInputNumber(SdlGfxFactory factory) : base(factory)
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
        public ANumberType NumberType { get; set; } = ANumberType.Float;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public int FontSize { get; set; } = 12;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (NumberType == ANumberType.Integer)
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
            return LingoSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
