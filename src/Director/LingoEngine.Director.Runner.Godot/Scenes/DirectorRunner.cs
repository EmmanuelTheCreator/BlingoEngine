using Godot;
using LingoEngine.Core;
using LingoEngine.Director.LGodot;
using LingoEngine.Net.RNetHost;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

public partial class DirectorRunner : Node
{
    public override void _Ready()
    {
        var services = new ServiceCollection();
        services.RegisterLingoEngine(c => c
                .WithRNetHostServer()
                .WithGlobalVarsDefault()
                .WithDirectorGodotEngine(this)
        );
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ILingoEngineRegistration>().Build(sp);
    }
}
