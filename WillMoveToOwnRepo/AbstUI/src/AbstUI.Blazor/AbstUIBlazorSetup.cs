using AbstUI.Blazor.Styles;
using AbstUI.Components;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Blazor;

public static class AbstUIBlazorSetup
{
    public static IServiceCollection WithAbstUIBlazor(this IServiceCollection services)
    {
        services
            .AddSingleton<IAbstFontManager, AbstBlazorFontManager>()
            .AddSingleton<IAbstStyleManager, AbstStyleManager>()
            .AddSingleton<IAbstBlazorStyleManager, AbstBlazorStyleManager>()
            .AddSingleton<AbstUIScriptResolver>()
            .AddSingleton<AbstBlazorComponentMapper>()
            .AddSingleton<AbstBlazorComponentContainer>()
            .AddSingleton<IAbstComponentFactory, AbstBlazorComponentFactory>();
        return services;
    }
}
