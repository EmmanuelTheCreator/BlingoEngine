using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Unity.Core;
using LingoEngine.Styles;
using LingoEngine.Unity.Styles;

namespace LingoEngine.Unity;

public static class UnityLingoSetup
{
    public static ILingoEngineRegistration WithUnityEngine(this ILingoEngineRegistration reg, Action<UnityFactory>? setup = null)
    {
        LingoEngineGlobal.RunFramework = LingoEngineRunFramework.Unity;
        reg.ServicesMain(s => s
                .AddSingleton<ILingoFrameworkFactory, UnityFactory>()
                .AddSingleton<ILingoFontManager, UnityFontManager>()
            )
           .WithFrameworkFactory(setup);
        return reg;
    }
}
