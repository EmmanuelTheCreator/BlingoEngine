using AbstUI.ImGui.Styles;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.ImGui;

/// <summary>
/// Helper extensions for wiring up the ImGui backend.
/// </summary>
public static class AbstUIImGuiSetup
{
    public static IServiceCollection WithAbstUIImGui(this IServiceCollection services)
    {
        services
            .AddSingleton<IAbstFontManager, ImGuiFontManager>()
            .AddSingleton<IAbstStyleManager, AbstStyleManager>();
        return services;
    }
}
