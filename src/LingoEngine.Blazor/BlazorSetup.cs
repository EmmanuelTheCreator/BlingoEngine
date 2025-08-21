using AbstUI;
using AbstUI.Blazor;
using AbstUI.Core;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Blazor;

public static class BlazorSetup
{
    public static ILingoEngineRegistration WithLingoBlazorEngine(this ILingoEngineRegistration reg, Action<BlazorFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
    {
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.Blazor;
        reg.ServicesMain(s => s
                .AddSingleton<ILingoFrameworkFactory, BlazorFactory>()
                .WithAbstUIBlazor(windowRegistrations)
            )
            .WithFrameworkFactory(setup)
            .AddPreBuildAction(x => x.WithAbstUIBlazor())
            ;
        return reg;
    }
}
