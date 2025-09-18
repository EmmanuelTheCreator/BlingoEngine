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
        private AbstLayoutWrapper _blingoLayoutWrapper;
        private readonly bool _canHandleEvent;

        public object FrameworkNode => this;

        public AbstSdlLayoutWrapper(AbstSdlComponentFactory factory, AbstLayoutWrapper layoutWrapper) : base(factory)
        {
            _blingoLayoutWrapper = layoutWrapper;
            _blingoLayoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;
            Name = content.Name + "_Wrapper";
            _canHandleEvent = layoutWrapper.Content.FrameworkObj is IHandleSdlEvent;
            if (content is AbstSdlComponent sdlComponent)
                sdlComponent.ComponentContext.SetParents(ComponentContext);
            
        }
        public override float Width
        {
            get => _blingoLayoutWrapper.Width;
            set
            {
                _blingoLayoutWrapper.Width = value;
                base.Width = value;
                ComponentContext.TargetWidth = (int)value;
            }
        }
        public override float Height
        {
            get => _blingoLayoutWrapper.Height;
            set
            {
                _blingoLayoutWrapper.Height = value;
                base.Height = value;
                ComponentContext.TargetHeight = (int)value;
            }
        }

        public AMargin Margin { get; set; }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            
            if (!Visibility) return default;
            
            var child = (AbstSdlComponent)_blingoLayoutWrapper.Content.FrameworkObj;
            var c = child.ComponentContext;
            var ox = c.OffsetX; var oy = c.OffsetY;
            c.OffsetX += (int)X;
            c.OffsetY += (int)Y;
            //Console.WriteLine($"WRAPPER BLIT {child.Name} at {child.X},{child.Y} into {Name} at {X},{Y}  off=({c.OffsetX},{c.OffsetY})");
            child.ComponentContext.RenderToTexture(context);
            c.OffsetX = ox; c.OffsetY = oy;


            ComponentContext.TargetWidth = (int)Width;
            ComponentContext.TargetHeight = (int)Height;
            return child.ComponentContext.Texture;

        }
        public bool CanHandleEvent(AbstSDLEvent e) => _canHandleEvent;
        public void HandleEvent(AbstSDLEvent e)
        {
            // Forward mouse events to children accounting for current scroll offset
            var sdlComponent = (AbstSdlComponent)_blingoLayoutWrapper.Content.FrameworkObj;
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

