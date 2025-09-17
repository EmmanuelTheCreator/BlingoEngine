using BlingoEngine.Director.SDL2;
using BlingoEngine.Net.RNetProjectHost;
using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
Console.Title = "Director c#";
var services = new ServiceCollection()
                // Add logging support
                .AddLogging(config =>
                {
                    config.AddConsole();   // log to console
                    config.SetMinimumLevel(LogLevel.Debug);
                });
services.RegisterBlingoEngine(c => c
    .WithRNetProjectHostServer()
    .WithGlobalVarsDefault()
    .WithDirectorSdlEngine("Director c#", 1280, 720)
);
var sp = services.BuildServiceProvider();
sp.GetRequiredService<IBlingoEngineRegistration>().Build(sp, false);
sp.GetRequiredService<BlingoSdlRootContext>().Run();
SdlSetup.Dispose();

