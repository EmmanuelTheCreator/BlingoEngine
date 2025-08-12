using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    public partial class LingoSdlLayoutWrapper : SdlGfxComponent, ILingoFrameworkGfxLayoutWrapper
    {
        private LingoGfxLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public LingoSdlLayoutWrapper(SdlGfxFactory factory,LingoGfxLayoutWrapper layoutWrapper):base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            layoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public LingoMargin Margin { get; set; }

        public override nint Render(LingoSDLRenderContext context)
        {
            return nint.Zero;
        }
    }
}
