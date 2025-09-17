using Godot;
using AbstUI.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.LGodot.Core;
using BlingoEngine.LGodot.Stages;
using BlingoEngine.LGodot.Styles;
using BlingoEngine.Setup;
using BlingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.LGodot;
using AbstUI;
namespace BlingoEngine.LGodot
{
    public static class BlingoGodotSetup
    {
        public static bool IsRegisteredServices = false;
        public static IBlingoEngineRegistration WithBlingoGodotEngine(this IBlingoEngineRegistration engineRegistration, Node rootNode, bool withStageInWindow = false, Action<GodotFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
        {
            if (IsRegisteredServices) return engineRegistration;
            IsRegisteredServices = true;
            AbstEngineGlobal.RunFramework = AbstEngineRunFramework.Godot;
            engineRegistration
                .ServicesMain(s => s
                        .AddGodotLogging()
                        .AddSingleton<BlingoGodotStyle>()
                        .AddSingleton<IBlingoFrameworkFactory, GodotFactory>()

                        .AddSingleton<IBlingoFrameworkStageContainer, BlingoGodotStageContainer>()
                        .AddSingleton(p => new BlingoGodotRootNode(rootNode, withStageInWindow))
                        .AddSingleton<IAbstGodotRootNode>(p => p.GetRequiredService<BlingoGodotRootNode>())
                        .WithAbstUIGodot(windowRegistrations)
                        )
                .AddPreBuildAction(p =>
                {
                    p.WithAbstUIGodot();
                })
                .WithFrameworkFactory(setup)
                ;
            return engineRegistration;
        }
      
    }
}

