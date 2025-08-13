using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxButton : SdlGfxComponent, ILingoFrameworkGfxButton, IDisposable
    {
        public SdlGfxButton(SdlGfxFactory factory) : base(factory)
        {
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public ILingoImageTexture? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            //ImGui.SetCursorPos(new Vector2(X, Y));
            var vpPos = context.ImGuiViewPort.WorkPos;
            var basePos = vpPos + new Vector2(X, Y);
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
