using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using AbstUI.Styles;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Texts;
using AbstUI.Blazor.Bitmaps;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorGfxCanvas : AbstBlazorComponent, IAbstFrameworkGfxCanvas, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly AbstBlazorComponentFactory _factory;
        private readonly IAbstFontManager _fontManager;
        private readonly int _width;
        private readonly int _height;
        private nint _texture;
        private readonly nint _imguiTexture;
        private readonly List<Action> _drawActions = new();
        private AColor? _clearColor;
        private bool _dirty;
        public object FrameworkNode => this;
        public nint Texture => _texture;

        public bool Pixilated { get; set; }

        public AbstBlazorGfxCanvas(AbstBlazorComponentFactory factory, IAbstFontManager fontManager, int width, int height) : base(factory)
        {
            _factory = factory;
            _fontManager = fontManager;
            _width = width;
            _height = height;
            _texture = Blazor.Blazor_CreateTexture(ComponentContext.Renderer, Blazor.Blazor_PIXELFORMAT_RGBA8888,
                (int)Blazor.Blazor_TextureAccess.Blazor_TEXTUREACCESS_TARGET, width, height);
            _imguiTexture = factory.RootContext.RegisterTexture(_texture);
            Width = width;
            Height = height;
            _dirty = true;
        }
        public override void Dispose()
        {
            base.Dispose();
            if (_texture != nint.Zero)
            {
                Blazor.Blazor_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
        }

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
