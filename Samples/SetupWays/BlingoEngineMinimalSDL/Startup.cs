using System;
using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlingoEngineMinimalSDL;

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

        services.RegisterBlingoEngine(configuration => configuration
            .WithBlingoSdlEngine("Minimal SDL Game", MinimalGameSDL.StageWidth, MinimalGameSDL.StageHeight)
            .SetProjectFactory<MinimalGameFactorySDL>()
            .BuildAndRunProject(sp => serviceProvider = sp));

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

