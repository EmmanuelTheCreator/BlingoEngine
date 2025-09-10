using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Containers
{
    public partial class AbstSdlLayoutWrapper : AbstSdlComponent, IAbstFrameworkLayoutWrapper, IFrameworkFor<AbstLayoutWrapper>, IHandleSdlEvent
    {
        private AbstLayoutWrapper _lingoLayoutWrapper;
        private readonly bool _canHandleEvent;

        public object FrameworkNode => this;

        public AbstSdlLayoutWrapper(AbstSdlComponentFactory factory, AbstLayoutWrapper layoutWrapper) : base(factory)
        {
            _lingoLayoutWrapper = layoutWrapper;
            _lingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
            Name = content.Name + "_Wrapper";
            _canHandleEvent = layoutWrapper.Content.FrameworkObj is IHandleSdlEvent;
            if (content is AbstSdlComponent sdlComponent)
                sdlComponent.ComponentContext.SetParents(ComponentContext);
            
        }
        public override float Width
        {
            get => _lingoLayoutWrapper.Width;
            set
            {
                _lingoLayoutWrapper.Width = value;
                base.Width = value;
                ComponentContext.TargetWidth = (int)value;
            }
        }
        public override float Height
        {
            get => _lingoLayoutWrapper.Height;
            set
            {
                _lingoLayoutWrapper.Height = value;
                base.Height = value;
                ComponentContext.TargetHeight = (int)value;
            }
        }

        public AMargin Margin { get; set; }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return default;
            var sdlComponent = (AbstSdlComponent)_lingoLayoutWrapper.Content.FrameworkObj;
            sdlComponent.X = X;
            sdlComponent.Y = Y;
            var renderResult = sdlComponent.Render(context);
            ComponentContext.TargetWidth = (int)Width;
            ComponentContext.TargetHeight = (int)Height;
            return renderResult;
        }
        public bool CanHandleEvent(AbstSDLEvent e) => _canHandleEvent;
        public void HandleEvent(AbstSDLEvent e)
        {
            // Forward mouse events to children accounting for current scroll offset
            var sdlComponent = (AbstSdlComponent)_lingoLayoutWrapper.Content.FrameworkObj;
            var oriX = e.OffsetX;
            var oriY = e.OffsetY;
            e.OffsetX += (int)Margin.Left;
            e.OffsetY += (int)Margin.Top;
            ContainerHelpers.HandleChildEvents(sdlComponent, e);
            e.OffsetX = oriX;
            e.OffsetY = oriY;
        }
    }
}
