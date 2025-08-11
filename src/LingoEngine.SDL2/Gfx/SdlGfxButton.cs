using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxButton : ILingoFrameworkGfxButton, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxButton(nint renderer)
        {
            _renderer = renderer;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;

        public object FrameworkNode => this;

        public event Action? Pressed;

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (ImGui.Button(Text, new Vector2(Width, Height)))
                Pressed?.Invoke();

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
        }

        public void Invoke() => Pressed?.Invoke();
        public void Dispose() { }
    }
}
