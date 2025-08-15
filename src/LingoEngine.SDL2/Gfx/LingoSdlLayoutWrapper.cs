using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    public partial class LingoSdlLayoutWrapper : SdlGfxComponent, ILingoFrameworkGfxLayoutWrapper
    {
        private LingoGfxLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public LingoSdlLayoutWrapper(SdlGfxFactory factory,LingoGfxLayoutWrapper layoutWrapper):base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            var sdlComponent = ((SdlGfxComponent)_lingoLayoutWrapper.Content.FrameworkObj);
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            return sdlComponent.Render(context);
        }
    }
}
