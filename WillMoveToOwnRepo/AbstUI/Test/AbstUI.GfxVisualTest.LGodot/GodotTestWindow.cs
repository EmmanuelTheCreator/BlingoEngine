using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.Windowing;
using System;

namespace AbstUI.GfxVisualTest.LGodot
{
    internal partial class GodotTestWindow : BaseGodotWindow, IFrameworkForInitializable<GfxTestWindow> , IFrameworkTestWindow
    {
        public GodotTestWindow(IServiceProvider serviceProvider) : base(GfxTestWindow.MyWindowCode, serviceProvider)
        {
        }

        public void Init(GfxTestWindow instance)
        {
            base.Init(instance);
        }

        
    }
}