using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if DEBUG_WITH_DIRECTOR
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.SDL2;
#endif
#if DEBUG
using LingoEngine.Net.RNetProjectHost;
using LingoEngine.Net.RNetPipeServer;
#endif

namespace LingoEngine.Demo.TetriGrounds.SDL2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            IServiceProvider? serviceProvider = null;
            services
                // Add logging support
                .AddLogging(config =>
                {
                    config.AddConsole();   // log to console
                    config.SetMinimumLevel(LogLevel.Debug);
                })

                .RegisterLingoEngine(c => c
#if DEBUG_WITH_DIRECTOR
                    .WithDirectorSdlEngine("TetriGrounds", 1600, 970, d =>
                    {
                        d.CsProjFile = "LingoEngine.Demo.TetriGrounds.Core\\LingoEngine.Demo.TetriGrounds.Core.csproj";
                    })
#else
                    .WithLingoSdlEngine("TetriGrounds", 730, 547)
#endif
#if DEBUG
                    //.WithRNetProjectHostServer(61699,true)
                    .WithRNetPipeHostServer(61699,true)
#endif
                    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject(sp => serviceProvider = sp)
                    );
            serviceProvider?.GetRequiredService<LingoSdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}
