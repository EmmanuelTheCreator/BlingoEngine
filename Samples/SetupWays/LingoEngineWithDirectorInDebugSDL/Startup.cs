using System;
using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#if DEBUG
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.SDL2;
#endif

namespace LingoEngineWithDirectorInDebugSDL;

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
        services.RegisterLingoEngine(configuration =>
        {
#if DEBUG
            configuration = configuration.WithDirectorSdlEngine(
                "SDL Director Sample",
                MinimalDirectorGame.DirectorWindowWidth,
                MinimalDirectorGame.DirectorWindowHeight,
                director =>
                {
                    director.CsProjFile = "LingoEngineWithDirectorInDebugSDL.csproj";
                });
#else
            configuration = configuration.WithLingoSdlEngine(
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
            serviceProvider?.GetRequiredService<LingoSdlRootContext>().Run();
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
