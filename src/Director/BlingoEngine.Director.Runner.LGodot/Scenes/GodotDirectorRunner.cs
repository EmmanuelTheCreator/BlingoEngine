using Godot;
using BlingoEngine.Core;
using BlingoEngine.Director.LGodot;
using BlingoEngine.Net.RNetPipeServer;
using BlingoEngine.Net.RNetPipeClient;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.Runner.LGodot.Scenes;
public partial class GodotDirectorRunner : Node
{
    public override void _Ready()
    {
        var services = new ServiceCollection();
        services.RegisterBlingoEngine(c => c
                .WithRNetPipeHostServer()
                //.WithRNetPipeClient()
                .WithGlobalVarsDefault()
                .WithDirectorGodotEngine(this)
        );
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IBlingoEngineRegistration>().Build(sp, false);
    }
}

