using AbstUI.Blazor.Components.Buttons;
using AbstUI.Blazor.Components.Containers;
using AbstUI.Blazor.Components.Graphics;
using AbstUI.Blazor.Components.Inputs;
using AbstUI.Blazor.Components.Texts;
using AbstUI.Blazor.Styles;
using AbstUI.Blazor.Windowing;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Styles;
using AbstUI.Resources;
using AbstUI.Blazor.Resources;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Blazor;

public static class AbstUIBlazorSetup
{
    public static IServiceCollection WithAbstUIBlazor(this IServiceCollection services, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
    {
        services
            .AddSingleton<IAbstFontManager, AbstBlazorFontManager>()
            .AddSingleton<IAbstStyleManager, AbstStyleManager>()
            .AddSingleton<IAbstResourceManager, BlazorResourceManager>()
            .AddSingleton<IAbstBlazorStyleManager, AbstBlazorStyleManager>()
            .AddSingleton<AbstUIScriptResolver>()
            .AddSingleton<AbstBlazorComponentMapper>()
            .AddSingleton<AbstBlazorComponentContainer>()
            .AddTransient<AbstBlazorWrapPanelComponent>()
            .AddTransient<AbstBlazorZoomBoxComponent>()
            .AddTransient<AbstBlazorPanelComponent>()
            .AddTransient<AbstBlazorTabContainerComponent>()
            .AddTransient<AbstBlazorTabItemComponent>()
            .AddTransient<AbstBlazorScrollContainerComponent>()
            .AddTransient<AbstBlazorButtonComponent>()
            .AddTransient<AbstBlazorLabelComponent>()
            .AddTransient<AbstBlazorInputTextComponent>()
            .AddTransient<AbstBlazorInputCheckboxComponent>()
            .AddTransient<AbstBlazorItemListComponent>()
            .AddTransient<AbstBlazorLayoutWrapperComponent>()
            .AddTransient<AbstBlazorGfxCanvasComponent>()
            .AddTransient<AbstBlazorColorPickerComponent>()
            .AddTransient(typeof(AbstBlazorInputNumberComponent<>))
            .AddTransient(typeof(AbstBlazorInputSliderComponent<>))
            .AddTransient<AbstBlazorSpinBoxComponent>()
            .AddTransient<AbstBlazorInputComboboxComponent>()
            .AddTransient<AbstBlazorStateButtonComponent>()
            .AddTransient<AbstBlazorHorizontalLineSeparatorComponent>()
            .AddTransient<AbstBlazorVerticalLineSeparatorComponent>()
            .AddSingleton<IAbstComponentFactory, AbstBlazorComponentFactory>()
            .AddSingleton<IAbstFrameworkWindowManager, AbstBlazorWindowManager>()
            .AddSingleton<IAbstFrameworkMainWindow, AbstBlazorMainWindow>()
            .AddTransient<IAbstFrameworkDialog, AbstBlazorDialog>()
            .WithAbstUI(windowRegistrations);
        return services;
    }

    public static IServiceProvider WithAbstUIBlazor(this IServiceProvider services)
    {
        services.WithAbstUI();// need to be first to register all the windows in the windows factory.
        var factory = services.GetRequiredService<IAbstComponentFactory>();
        factory
            .Register<AbstMainWindow, AbstBlazorMainWindow>()
            .Register<AbstWindowManager, AbstBlazorWindowManager>()
            .Register<AbstDialog, AbstBlazorDialog>();
        return services;
    }
}
