using System.Numerics;
using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.ImGui.Bitmaps;
using AbstUI.Styles;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiStateButton : AbstImGuiComponent, IAbstFrameworkStateButton, IDisposable
    {
        private readonly AbstImGuiComponentFactory _factory;
        private IAbstTexture2D? _textureOn;
        private IAbstTexture2D? _textureOff;
        private nint _texOnHandle;
        private nint _texOffHandle;

        public AbstImGuiStateButton(AbstImGuiComponentFactory factory) : base(factory)
        {
            _factory = factory;
        }
        public bool Enabled { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
        private AColor _borderHoverColor = AbstDefaultColors.Button_Border_Hover;
        private AColor _borderPressedColor = AbstDefaultColors.Button_Border_Pressed;
        private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
        private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
        private AColor _backgroundPressedColor = AbstDefaultColors.Button_Bg_Pressed;
        private AColor _textColor = AColor.FromRGB(0,0,0);
        public AColor BorderColor { get => _borderColor; set => _borderColor = value; }
        public AColor BorderHoverColor { get => _borderHoverColor; set => _borderHoverColor = value; }
        public AColor BorderPressedColor { get => _borderPressedColor; set => _borderPressedColor = value; }
        public AColor BackgroundColor { get => _backgroundColor; set => _backgroundColor = value; }
        public AColor BackgroundHoverColor { get => _backgroundHoverColor; set => _backgroundHoverColor = value; }
        public AColor BackgroundPressedColor { get => _backgroundPressedColor; set => _backgroundPressedColor = value; }
        public AColor TextColor { get => _textColor; set => _textColor = value; }
        public IAbstTexture2D? TextureOn
        {
            get => _textureOn;
            set
            {
                _textureOn = value;
                _texOnHandle = RegisterTexture(value);
            }
        }

        public IAbstTexture2D? TextureOff
        {
            get => _textureOff;
            set
            {
                _textureOff = value;
                _texOffHandle = RegisterTexture(value);
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

        private nint RegisterTexture(IAbstTexture2D? texture)
        {
            return texture switch
            {
                ImGuiTexture2D img when img.Handle != nint.Zero => img.Handle,
                ImGuiImageTexture img => _factory.RootContext.RegisterTexture(img.SurfaceId),
                _ => nint.Zero
            };
        }


        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            global::ImGuiNET.ImGui.SetCursorPos(new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();

            nint tex = _isOn ? _texOnHandle : _texOffHandle;
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
            base.Dispose();
        }
    }
}
