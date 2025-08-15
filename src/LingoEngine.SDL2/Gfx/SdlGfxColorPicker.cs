using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxColorPicker : SdlGfxComponent, ILingoFrameworkGfxColorPicker, IDisposable
    {
        public SdlGfxColorPicker(SdlGfxFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color;
        public AColor Color
        {
            get => _color;
            set
            {
                if (!_color.Equals(value))
                {
                    _color = value;
                    ValueChanged?.Invoke();
                }
            }
        }

        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            Vector3 col = new Vector3(_color.R / 255f, _color.G / 255f, _color.B / 255f);
            if (ImGui.ColorEdit3("##color", ref col))
            {
                Color = new AColor((byte)(col.X * 255), (byte)(col.Y * 255), (byte)(col.Z * 255));
            }

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
            return LingoSDLRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
