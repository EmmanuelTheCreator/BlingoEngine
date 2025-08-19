using Godot;
using AbstUI.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Core;
using LingoEngine.LGodot.Stages;
using LingoEngine.LGodot.Styles;
using LingoEngine.Setup;
using LingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.LGodot;
using LingoEngine.Core;
using AbstUI;
namespace LingoEngine.LGodot
{
    public static class LingoGodotSetup
    {
        public static ILingoEngineRegistration WithLingoGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode, bool withStageInWindow = false, Action<GodotFactory>? setup = null)
        {
            LingoEngineGlobal.RunFramework = AbstEngineRunFramework.Godot;
            engineRegistration
                .ServicesMain(s => s
                        .AddGodotLogging()
                        .AddSingleton<LingoGodotStyle>()
                        .AddSingleton<ILingoFrameworkFactory, GodotFactory>()

                        .AddSingleton<ILingoFrameworkStageContainer, LingoGodotStageContainer>()
                        .AddSingleton(p => new LingoGodotRootNode(rootNode, withStageInWindow))
                        .AddSingleton<IAbstGodotRootNode>(p => p.GetRequiredService<LingoGodotRootNode>())
                        .WithAbstUIGodot()
                        )
                .WithFrameworkFactory(setup)
                
                ;
            return engineRegistration;
        }

    }
}
