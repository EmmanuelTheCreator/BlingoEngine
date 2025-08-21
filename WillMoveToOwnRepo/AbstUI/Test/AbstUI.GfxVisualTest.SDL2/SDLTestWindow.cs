using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Windowing;

namespace AbstUI.GfxVisualTest.SDL2
{
    public class SDLTestWindow : AbstSdlWindow, IFrameworkTestWindow
    {
        private GfxTestWindow _instance = null!;

        public SDLTestWindow(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public void Init(GfxTestWindow instance)
        {
            _instance = instance;
            instance.Init(this);
            
        }
    }
}
