using AbstUI.Styles;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.GfxVisualTest
{
    public static class GfxTestSetup
    {
        public static void SetupGfxTest(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddTransient<GfxTestWindow>()
                ;
        }
        public static void SetupGfxTest(this IServiceProvider services)
        {
            services.GetRequiredService<IAbstFontManager>().LoadAll();
            services.GetRequiredService<IAbstWindowManager>()
                .Register<GfxTestWindow>(GfxTestWindow.MyWindowCode);
        }
    }
}
