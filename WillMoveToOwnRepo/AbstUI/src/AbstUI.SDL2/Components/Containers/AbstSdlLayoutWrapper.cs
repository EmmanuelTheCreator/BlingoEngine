using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Containers
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
