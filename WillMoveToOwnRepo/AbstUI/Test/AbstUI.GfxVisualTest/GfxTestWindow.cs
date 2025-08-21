using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.Windowing;

namespace AbstUI.GfxVisualTest
{
    public interface IFrameworkTestWindow : IAbstFrameworkWindow,  IFrameworkForInitializable<GfxTestWindow>
    {

    }
    public class GfxTestWindow : AbstWindow<IFrameworkTestWindow>
    {
        public const string MyWindowCode = "GfxTestWindow";
        public GfxTestWindow(IServiceProvider serviceProvider) : base(serviceProvider, MyWindowCode)
        {
            Width = 600;
            Height = 300;
            MinimumHeight = 200;
            MinimumWidth = 400;
            Title = "Gfx Test Window";
        }

        public override void Init(IAbstFrameworkWindow frameworkWindow)
        {
            base.Init(frameworkWindow);
            var content = _componentFactory.CreateScrollContainer("scroll");
            var wrap = _componentFactory.CreateWrapPanel(AOrientation.Vertical, "wrap");
            wrap.Compose()
                .AddLabel("Label1", "my window content test 1")
                .Finalize();
            ;

            content.AddItem(wrap);
            Content = content;
        }
       
    }
}
