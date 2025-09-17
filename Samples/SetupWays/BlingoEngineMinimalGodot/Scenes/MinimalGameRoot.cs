using System;
using Godot;
using BlingoEngine.LGodot;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngineMinimalGodot.Scenes;

public partial class MinimalGameRoot : Node2D
{
    private IServiceProvider? _serviceProvider;

    public override void _Ready()
    {
        try
        {
            ProjectSettings.SetSetting("display/window/stretch/mode", "disabled");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "ignore");
            DisplayServer.WindowSetSize(new Vector2I(MinimalGame.StageWidth, MinimalGame.StageHeight));

            var services = new ServiceCollection();

            services.RegisterBlingoEngine(configuration => configuration
                .WithBlingoGodotEngine(this)
                .SetProjectFactory<MinimalGameFactoryGodot>()
                .BuildAndRunProject(sp => _serviceProvider = sp));
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex);
        }
    }

    public override void _ExitTree()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

