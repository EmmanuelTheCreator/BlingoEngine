using AbstUI.ImGui.Styles;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.ImGui
{
    public static class AbstUIImGuiSetup
    {
        public static IServiceCollection WithAbstUIImGui(this IServiceCollection services)
        {
            services
                .AddSingleton<IAbstFontManager, ImGuiFontManager>()
                        ;

            return services;
        }
    }
}
