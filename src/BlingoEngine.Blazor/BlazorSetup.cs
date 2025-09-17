using AbstUI;
using AbstUI.Blazor;
using AbstUI.Core;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Setup;
using BlingoEngine.Blazor.Movies;
using BlingoEngine.Blazor.Stages;
using BlingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Blazor;

public static class BlazorSetup
{
    public static IBlingoEngineRegistration WithBlingoBlazorEngine(this IBlingoEngineRegistration reg, Action<BlazorFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
    {
        BlingoEngineGlobal.RunFramework = AbstEngineRunFramework.Blazor;
        reg.ServicesMain(s => s
                .AddSingleton<IBlingoFrameworkFactory, BlazorFactory>()
                .AddSingleton<BlingoBlazorRootPanel>()
                .AddSingleton<IBlingoFrameworkStageContainer, BlingoBlazorStageContainer>()
                .WithAbstUIBlazor(windowRegistrations)
            )
            .WithFrameworkFactory(setup)
            .AddPreBuildAction(x => x.WithAbstUIBlazor())
            .AddBuildAction(sp => sp.GetRequiredService<BlingoPlayer>().MediaRequiresAsyncPreload = true)
            ;
        return reg;
    }
}

