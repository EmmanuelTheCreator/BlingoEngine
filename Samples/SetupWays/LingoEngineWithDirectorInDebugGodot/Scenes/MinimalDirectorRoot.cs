using System;
using Godot;
using LingoEngine.LGodot;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.LGodot;
#endif

namespace LingoEngineWithDirectorInDebugGodot.Scenes;

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

            services.RegisterLingoEngine(configuration =>
            {
#if DEBUG
                configuration = configuration.WithDirectorGodotEngine(this, directorSettings =>
                {
                    directorSettings.CsProjFile = "LingoEngineWithDirectorInDebugGodot.csproj";
                });
#else
                configuration = configuration.WithLingoGodotEngine(this);
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
