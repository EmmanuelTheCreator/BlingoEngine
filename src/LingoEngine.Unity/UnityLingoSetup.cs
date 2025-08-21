using AbstUI;
using AbstUI.Core;
using AbstUI.LUnity;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using LingoEngine.Stages;
using LingoEngine.Unity.Core;
using LingoEngine.Unity.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Unity;

public static class UnityLingoSetup
{
    public static ILingoEngineRegistration WithUnityEngine(this ILingoEngineRegistration reg, Action<UnityFactory>? setup = null)
    {
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.Unity;
        reg.ServicesMain(s => s
                .AddSingleton<ILingoFrameworkFactory, UnityFactory>()
                .AddSingleton<ILingoFrameworkStageContainer, UnityStageContainer>()
                .WithAbstUIUnity()
            )
           .WithFrameworkFactory(setup)
           .AddPreBuildAction(x => x.WithUnityEngine())
           ;
        return reg;
    }

    private static IServiceProvider WithUnityEngine(this IServiceProvider services)
    {
        services.WithAbstUIUnity();
        return services;
    }
}
