using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.SDL2.Core;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxButton : SdlGfxComponent, ILingoFrameworkGfxButton, IDisposable
    {
        public SdlGfxButton(SdlFactory factory) : base(factory)
        {
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        private ILingoImageTexture? _icon;
        public ILingoImageTexture? IconTexture { get => _icon; set => _icon = value; }

        public object FrameworkNode => this;

        public ILingoImageTexture? IconTexture { get; set; }

        public event Action? Pressed;

        public override nint Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (ImGui.Button(Text, new Vector2(Width, Height)))
                Pressed?.Invoke();

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return nint.Zero;
        }

        public void Invoke() => Pressed?.Invoke();
        public override void Dispose() => base.Dispose();
    }
}
