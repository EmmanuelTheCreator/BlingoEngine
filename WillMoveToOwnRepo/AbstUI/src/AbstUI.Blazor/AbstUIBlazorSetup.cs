using AbstUI.Blazor.Components.Containers;
using AbstUI.Blazor.Styles;
using AbstUI.Blazor.Windowing;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Styles;
using AbstUI.Windowing;
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
            .AddSingleton<IAbstComponentFactory, AbstBlazorComponentFactory>()
            .AddSingleton<IAbstFrameworkMainWindow, AbstBlazorMainWindow>()
            .WithAbstUI();
        return services;
    }
}
