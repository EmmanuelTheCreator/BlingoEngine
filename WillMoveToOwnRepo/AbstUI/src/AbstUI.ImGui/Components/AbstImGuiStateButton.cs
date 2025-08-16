using System.Numerics;
using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.ImGui.Bitmaps;
using AbstUI.ImGui.ImGuiLL;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiStateButton : AbstImGuiComponent, IAbstFrameworkStateButton, IDisposable
    {
        private nint _textureOnPtr;
        private IAbstTexture2D? _textureOn;
        private nint _textureOffPtr;
        private IAbstTexture2D? _textureOff;

        public AbstImGuiStateButton(AbstImGuiComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        public IAbstTexture2D? TextureOn
        {
            get => _textureOn;
            set
            {
                _textureOn = value;
                if (_textureOnPtr != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_textureOnPtr);
                    _textureOnPtr = nint.Zero;
                }
                if (value is ImGuiImageTexture img)
                {
                    _textureOnPtr = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
                }
            }
        }

        public IAbstTexture2D? TextureOff
        {
            get => _textureOff;
            set
            {
                _textureOff = value;
                if (_textureOffPtr != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_textureOffPtr);
                    _textureOffPtr = nint.Zero;
                }
                if (value is ImGuiImageTexture img)
                {
                    _textureOffPtr = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
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
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;


        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            global::ImGuiNET.ImGui.SetCursorPos(new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();

            nint tex = _isOn ? _textureOnPtr : _textureOffPtr;
            if (tex != nint.Zero)
            {
                Vector4 bg = _isOn ? new Vector4(0.25f, 0.25f, 0.25f, 1f) : Vector4.Zero;
                if (global::ImGuiNET.ImGui.ImageButton(Name, tex, new Vector2(Width, Height), Vector2.Zero, Vector2.One, bg, Vector4.One))
                    IsOn = !_isOn;
            }
            else
            {
                bool value = _isOn;
                if (global::ImGuiNET.ImGui.Checkbox(Text, ref value))
                    IsOn = value;
            }

            if (!Enabled)
                global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopID();
            return nint.Zero;
        }

        public override void Dispose()
        {
            if (_textureOnPtr != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_textureOnPtr);
                _textureOnPtr = nint.Zero;
            }
            if (_textureOffPtr != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_textureOffPtr);
                _textureOffPtr = nint.Zero;
            }
            base.Dispose();
        }
    }
}
