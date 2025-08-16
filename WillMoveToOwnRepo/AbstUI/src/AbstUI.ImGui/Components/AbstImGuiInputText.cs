using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiInputText : AbstImGuiComponent, IAbstFrameworkInputText, IDisposable
    {
        public AbstImGuiInputText(AbstImGuiComponentFactory factory, bool multiLine) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public int MaxLength { get; set; }
        public string? Font { get; set; }
        public int FontSize { get; set; } = 12;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public bool IsMultiLine { get; set; }

        public event Action? ValueChanged;

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            // place relative to current context origin
            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));

            if (Width > 0) global::ImGuiNET.ImGui.SetNextItemWidth(Width);

            global::ImGuiNET.ImGui.PushID(Name);
            if (!Enabled) global::ImGuiNET.ImGui.BeginDisabled();

            uint cap = MaxLength > 0 ? (uint)MaxLength : 1024u;
            if (global::ImGuiNET.ImGui.InputText("##text", ref _text, cap))
                ValueChanged?.Invoke();

            if (!Enabled) global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }


        public override void Dispose() => base.Dispose();
    }
}
