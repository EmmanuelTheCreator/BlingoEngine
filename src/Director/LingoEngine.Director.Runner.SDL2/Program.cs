using LingoEngine.Net.RNetHost;
using LingoEngine.Director.SDL2;
using LingoEngine.Setup;
using LingoEngine.SDL2;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.RegisterLingoEngine(c => c
    .WithRNetHostServer()
    .WithGlobalVarsDefault()
    .WithDirectorSdlEngine("Director c#", 1280, 720)
);
var sp = services.BuildServiceProvider();
sp.GetRequiredService<ILingoEngineRegistration>().Build(sp);
sp.GetRequiredService<LingoSdlRootContext>().Run();
SdlSetup.Dispose();
