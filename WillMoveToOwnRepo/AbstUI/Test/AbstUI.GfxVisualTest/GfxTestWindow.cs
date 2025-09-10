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
            X = 30;
            Y = 40;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            var content = _componentFactory.CreateScrollContainer("scroll2");

            //var wrap = _componentFactory.CreatePanel("wrap");
            ////wrap.Compose(_componentFactory)
            ////    .AddLabel("Label1", "my window content test 1")
            ////    .Finalize();
            ////;
            var wrap = _componentFactory.CreateWrapPanel(AOrientation.Vertical, "wrap2");
            wrap.Width = 550;
            wrap.Height = 280;
            wrap.Compose()
                .AddLabel("Label1", "my window content test 1")
                .AddLabel("Label2", "my window content test 2")
                .AddLabel("Label3", "my window content test 3")
                .Finalize();
            ;

            var btn11 = _componentFactory.CreateButton("btn1", "Button 1");
            wrap.AddItem(btn11);
            //var factory = _componentFactory;
            //content.AddItem(wrap);
            //var tabs = factory.CreateTabContainer("tabContainer");
            //tabs.Width = 300;
            //tabs.Height = 120;
            //var tab1 = factory.CreateTabItem("tab1", "First tab");
            //var tab1Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab1panel");
            //tab1Panel.AddItem(factory.CreateButton("tab1Btn1", "Button 1"));
            //tab1.Content = tab1Panel;
            //tabs.AddTab(tab1);
            //var tab2 = factory.CreateTabItem("tab2", "Second tab");
            //var tab2Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab2panel");
            //tab2Panel.AddItem(factory.CreateButton("tab2Btn2", "Button 2"));
            //tab2.Content = tab2Panel;
            //tabs.AddTab(tab2);

            //content.AddItem(tabs);
            Content = wrap;
            Title = "Gfx Test Window";
        }
       
    }
}
