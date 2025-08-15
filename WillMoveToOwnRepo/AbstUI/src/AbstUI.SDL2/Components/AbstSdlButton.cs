using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlButton : AbstSdlComponent, IAbstFrameworkButton, IDisposable
    {
        public AbstSdlButton(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public IAbstTexture2D? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            var basePos = context.Origin + new Vector2(X, Y);
            ImGui.SetCursorScreenPos(basePos);
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (ImGui.Button(Text, new Vector2(Width, Height)))
                Pressed?.Invoke();

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return AbstSDLRenderResult.RequireRender();
        }

        public void Invoke() => Pressed?.Invoke();
        public override void Dispose() => base.Dispose();
    }
}
