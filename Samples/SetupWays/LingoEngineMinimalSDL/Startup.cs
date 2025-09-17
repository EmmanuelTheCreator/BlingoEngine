using System;
using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LingoEngineMinimalSDL;

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

        services.RegisterLingoEngine(configuration => configuration
            .WithLingoSdlEngine("Minimal SDL Game", MinimalGameSDL.StageWidth, MinimalGameSDL.StageHeight)
            .SetProjectFactory<MinimalGameFactorySDL>()
            .BuildAndRunProject(sp => serviceProvider = sp));

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
