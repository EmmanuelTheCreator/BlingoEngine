using Godot;
using LingoEngine.Director.LGodot;
using LingoEngine.Net.RNetHost;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

public partial class DirectorRunner : Node
{
    public override void _Ready()
    {
        var services = new ServiceCollection();
        services.RegisterLingoEngine(c =>
        {
            c.WithDirectorHostServer();
            c.WithDirectorGodotEngine(this);
        });
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ILingoEngineRegistration>().Build(sp);
    }
}
