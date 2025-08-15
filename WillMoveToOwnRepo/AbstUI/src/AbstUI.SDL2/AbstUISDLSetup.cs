using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.SDL2
{
    public static class AbstUISDLSetup
    {
        public static IServiceCollection WithAbstUISdl(this IServiceCollection services)
        {
            services
                .AddSingleton<IAbstFontManager, SdlFontManager>()
                        ;

            return services;
        }
    }
}
