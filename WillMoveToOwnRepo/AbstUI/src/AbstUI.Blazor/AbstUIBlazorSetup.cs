using AbstUI.Blazor.Styles;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Blazor;

public static class AbstUIBlazorSetup
{
    public static IServiceCollection WithAbstUIBlazor(this IServiceCollection services)
    {
        services
            .AddSingleton<IAbstFontManager, AbstBlazorFontManager>()
            .AddSingleton<IAbstBlazorStyleManager, AbstBlazorStyleManager>();
        return services;
    }
}
