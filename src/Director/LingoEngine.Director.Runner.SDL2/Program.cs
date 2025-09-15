using LingoEngine.Director.SDL2;
using LingoEngine.Net.RNetHost;
using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
Console.Title = "Lingo Director";
var services = new ServiceCollection()
                // Add logging support
                .AddLogging(config =>
                {
                    config.AddConsole();   // log to console
                    config.SetMinimumLevel(LogLevel.Debug);
                });
services.RegisterLingoEngine(c => c
    .WithRNetHostServer()
    .WithGlobalVarsDefault()
    .WithDirectorSdlEngine("Director c#", 1280, 720)
);
var sp = services.BuildServiceProvider();
sp.GetRequiredService<ILingoEngineRegistration>().Build(sp, false);
sp.GetRequiredService<LingoSdlRootContext>().Run();
SdlSetup.Dispose();
