using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxButton : SdlGfxComponent, ILingoFrameworkGfxButton, IDisposable
    {
        public SdlGfxButton(SdlGfxFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public ILingoTexture2D? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
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
            return LingoSDLRenderResult.RequireRender();
        }

        public void Invoke() => Pressed?.Invoke();
        public override void Dispose() => base.Dispose();
    }
}
