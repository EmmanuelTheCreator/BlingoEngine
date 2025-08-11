using System;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxStateButton : ILingoFrameworkGfxStateButton, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;
        private nint _texturePtr;
        private ILingoImageTexture? _texture;

        public SdlGfxStateButton(nint renderer)
        {
            _renderer = renderer;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        public Bitmaps.ILingoImageTexture? TextureOn
        {
            get => _texture;
            set
            {
                _texture = value;
                if (_texturePtr != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_texturePtr);
                    _texturePtr = nint.Zero;
                }
                if (value is SdlImageTexture img)
                {
                    _texturePtr = SDL.SDL_CreateTextureFromSurface(_renderer, img.SurfaceId);
                }
            }
        }
        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public ILingoImageTexture? TextureOff { get; set; }

        public void Render()
        {
            if (!Visibility) return;
            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            if (!Enabled)
                ImGui.BeginDisabled();

            if (_texturePtr != nint.Zero)
            {
                Vector4 bg = _isOn ? new Vector4(0.25f, 0.25f, 0.25f, 1f) : Vector4.Zero;
                if (ImGui.ImageButton(Name, _texturePtr, new Vector2(Width, Height), Vector2.Zero, Vector2.One, bg, Vector4.One))
                    IsOn = !_isOn;
            }
            else
            {
                bool value = _isOn;
                if (ImGui.Checkbox(Text, ref value))
                    IsOn = value;
            }

            if (!Enabled)
                ImGui.EndDisabled();
            ImGui.PopID();
        }

        public void Dispose()
        {
            if (_texturePtr != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texturePtr);
                _texturePtr = nint.Zero;
            }
        }
    }
}
