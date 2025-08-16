using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    public partial class AbstImGuiLayoutWrapper : AbstImGuiComponent, IAbstFrameworkLayoutWrapper
    {
        private AbstLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public AbstImGuiLayoutWrapper(AbstImGuiComponentFactory factory, AbstLayoutWrapper layoutWrapper) : base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            var sdlComponent = (AbstImGuiComponent)_lingoLayoutWrapper.Content.FrameworkObj;
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            return sdlComponent.Render(context);
        }
    }
}
