using System.Numerics;
using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Blazor.Bitmaps;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorStateButton : AbstBlazorComponent, IAbstFrameworkStateButton, IDisposable
    {
        private nint _textureOnPtr;
        private IAbstTexture2D? _textureOn;
        private nint _textureOffPtr;
        private IAbstTexture2D? _textureOff;

        public AbstBlazorStateButton(AbstBlazorComponentFactory factory) : base(factory)
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
                    Blazor.Blazor_DestroyTexture(_textureOnPtr);
                    _textureOnPtr = nint.Zero;
                }
                if (value is BlazorImageTexture img)
                {
                    _textureOnPtr = Blazor.Blazor_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
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
                    Blazor.Blazor_DestroyTexture(_textureOffPtr);
                    _textureOffPtr = nint.Zero;
                }
                if (value is BlazorImageTexture img)
                {
                    _textureOffPtr = Blazor.Blazor_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
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


        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
