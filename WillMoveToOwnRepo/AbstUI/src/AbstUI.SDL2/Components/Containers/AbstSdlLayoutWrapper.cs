using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    public partial class AbstSdlLayoutWrapper : AbstSdlComponent, IAbstFrameworkLayoutWrapper
    {
        private AbstLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public AbstSdlLayoutWrapper(AbstSdlComponentFactory factory, AbstLayoutWrapper layoutWrapper) : base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            var sdlComponent = (AbstSdlComponent)_lingoLayoutWrapper.Content.FrameworkObj;
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            return sdlComponent.Render(context);
        }
    }
}
