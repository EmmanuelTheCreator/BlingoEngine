using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    {
        private AbstLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
        }

        public AMargin Margin { get; set; }

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
