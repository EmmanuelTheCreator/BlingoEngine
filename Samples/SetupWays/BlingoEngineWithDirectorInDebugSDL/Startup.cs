using System;
using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#if DEBUG
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.SDL2;
#endif

namespace BlingoEngineWithDirectorInDebugSDL;

internal static class Startup
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        IServiceProvider? serviceProvider = null;
        services.AddLogging(config =>
        {
            //config.AddConsole();   // log to console
            config.SetMinimumLevel(LogLevel.Debug);
        });
        services.RegisterBlingoEngine(configuration =>
        {
#if DEBUG
            configuration = configuration.WithDirectorSdlEngine(
                "SDL Director Sample",
                MinimalDirectorGame.DirectorWindowWidth,
                MinimalDirectorGame.DirectorWindowHeight,
                director =>
                {
                    director.CsProjFile = "BlingoEngineWithDirectorInDebugSDL.csproj";
                });
#else
            configuration = configuration.WithBlingoSdlEngine(
                "SDL Director Sample",
                MinimalDirectorGame.RuntimeWindowWidth,
                MinimalDirectorGame.RuntimeWindowHeight);
#endif

            configuration
                .SetProjectFactory<MinimalDirectorGameFactorySDL>()
                .BuildAndRunProject(sp => serviceProvider = sp);
        });

        try
        {
            serviceProvider?.GetRequiredService<BlingoSdlRootContext>().Run();
        }
        finally
        {
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            SdlSetup.Dispose();
        }
    }
}

