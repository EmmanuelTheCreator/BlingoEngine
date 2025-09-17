using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    public partial class AbstImGuiLayoutWrapper : AbstImGuiComponent, IAbstFrameworkLayoutWrapper
    {
        private AbstLayoutWrapper _blingoLayoutWrapper;
        public object FrameworkNode => this;

        public AbstImGuiLayoutWrapper(AbstImGuiComponentFactory factory, AbstLayoutWrapper layoutWrapper) : base(factory)
        {
            _blingoLayoutWrapper = layoutWrapper;
            _blingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            var sdlComponent = (AbstImGuiComponent)_blingoLayoutWrapper.Content.FrameworkObj;
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            return sdlComponent.Render(context);
        }
    }
}

