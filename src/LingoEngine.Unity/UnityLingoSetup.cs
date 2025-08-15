using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Unity.Core;
using AbstUI.Core;
using AbstUI.LUnity;
using LingoEngine.Core;

namespace LingoEngine.Unity;

public static class UnityLingoSetup
{
    public static ILingoEngineRegistration WithUnityEngine(this ILingoEngineRegistration reg, Action<UnityFactory>? setup = null)
    {
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.Unity;
        reg.ServicesMain(s => s
                .AddSingleton<ILingoFrameworkFactory, UnityFactory>()
                .WithAbstUIUnity()
            )
           .WithFrameworkFactory(setup);
        return reg;
    }
}
