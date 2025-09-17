using System;
using Godot;
using BlingoEngine.LGodot;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.LGodot;
#endif

namespace BlingoEngineWithDirectorInDebugGodot.Scenes;

public partial class MinimalDirectorRoot : Node2D
{
    private IServiceProvider? _serviceProvider;

    public override void _Ready()
    {
        try
        {
            ProjectSettings.SetSetting("display/window/stretch/mode", "disabled");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "ignore");
#if DEBUG
            DisplayServer.WindowSetSize(new Vector2I(
                MinimalDirectorGame.DirectorWindowWidth,
                MinimalDirectorGame.DirectorWindowHeight));
#else
            DisplayServer.WindowSetSize(new Vector2I(
                MinimalDirectorGame.RuntimeWindowWidth,
                MinimalDirectorGame.RuntimeWindowHeight));
#endif

            var services = new ServiceCollection();

            services.RegisterBlingoEngine(configuration =>
            {
#if DEBUG
                configuration = configuration.WithDirectorGodotEngine(this, directorSettings =>
                {
                    directorSettings.CsProjFile = "BlingoEngineWithDirectorInDebugGodot.csproj";
                });
#else
                configuration = configuration.WithBlingoGodotEngine(this);
#endif

                configuration
                    .SetProjectFactory<MinimalDirectorGameFactoryGodot>()
                    .BuildAndRunProject(sp => _serviceProvider = sp);
            });
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

