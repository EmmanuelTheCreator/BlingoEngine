using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    public partial class SdlLayoutWrapper : SdlGfxComponent, IAbstUIFrameworkGfxLayoutWrapper
    {
        private AbstUIGfxLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public SdlLayoutWrapper(SdlGfxFactory factory, AbstUIGfxLayoutWrapper layoutWrapper) : base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            var sdlComponent = (SdlGfxComponent)_lingoLayoutWrapper.Content.FrameworkObj;
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            return sdlComponent.Render(context);
        }
    }
}
