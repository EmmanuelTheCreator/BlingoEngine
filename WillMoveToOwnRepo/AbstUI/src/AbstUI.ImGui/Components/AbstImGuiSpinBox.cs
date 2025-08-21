using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiSpinBox : AbstImGuiComponent, IAbstFrameworkSpinBox, IHasTextBackgroundBorderColor, IDisposable
    {
        public AbstImGuiSpinBox(AbstImGuiComponentFactory factory) : base(factory)
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
        public AColor TextColor { get; set; } = AColors.Black;
        public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Text, TextColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.FrameBg, BackgroundColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, BorderColor.ToImGuiColor());
            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();
            float val = _value;
            if (global::ImGuiNET.ImGui.InputFloat("##spin", ref val, 1f))
            {
                val = Math.Clamp(val, Min, Max);
                Value = val;
            }
            if (!Enabled)
                global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopStyleColor(3);
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
