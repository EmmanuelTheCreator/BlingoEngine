using AbstUI;
using AbstUI.Core;
using AbstUI.LUnity;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Setup;
using BlingoEngine.Stages;
using BlingoEngine.Unity.Core;
using BlingoEngine.Unity.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Unity;

public static class UnityBlingoSetup
{
    public static IBlingoEngineRegistration WithUnityEngine(this IBlingoEngineRegistration reg, Action<UnityFactory>? setup = null)
    {
        BlingoEngineGlobal.RunFramework = AbstEngineRunFramework.Unity;
        reg.ServicesMain(s => s
                .AddSingleton<IBlingoFrameworkFactory, UnityFactory>()
                .AddSingleton<IBlingoFrameworkStageContainer, UnityStageContainer>()
                .AddUnityLogging()
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

