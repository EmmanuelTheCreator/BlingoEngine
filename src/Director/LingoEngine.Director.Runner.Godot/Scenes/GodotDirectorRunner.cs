using Godot;
using LingoEngine.Core;
using LingoEngine.Director.LGodot;
using LingoEngine.Net.RNetPipeServer;
using LingoEngine.Net.RNetPipeClient;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

public partial class GodotDirectorRunner : Node
{
    public override void _Ready()
    {
        var services = new ServiceCollection();
        services.RegisterLingoEngine(c => c
                .WithRNetPipeHostServer()
                //.WithRNetPipeClient()
                .WithGlobalVarsDefault()
                .WithDirectorGodotEngine(this)
        );
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ILingoEngineRegistration>().Build(sp, false);
    }
}
