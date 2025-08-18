using AbstUI.Blazor;
using AbstUI.Core;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Blazor;

public static class BlazorSetup
{
    public static ILingoEngineRegistration WithLingoBlazorEngine(this ILingoEngineRegistration reg, Action<BlazorFactory>? setup = null)
    {
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.Blazor;
        reg.ServicesMain(s => s
                .AddSingleton<ILingoFrameworkFactory, BlazorFactory>()
                .WithAbstUIBlazor()
            )
            .WithFrameworkFactory(setup);
        return reg;
    }
}
