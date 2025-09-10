using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG_WITH_DIRECTOR
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.SDL2;
#endif

namespace LingoEngine.Demo.TetriGrounds.SDL2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            IServiceProvider? serviceProvider = null;
#if DEBUG_WITH_DIRECTOR
            services.RegisterLingoEngine(c => c
                    .WithDirectorSdlEngine("TetriGrounds", 1600, 970, d =>
                    {
                        d.CsProjFile = "LingoEngine.Demo.TetriGrounds.Core\\LingoEngine.Demo.TetriGrounds.Core.csproj";
                    })
                    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject(sp => serviceProvider = sp)
                    );
#else
            services.RegisterLingoEngine(c => c
                    .WithLingoSdlEngine("TetriGrounds", 730, 547)
                    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject(sp => serviceProvider = sp)
                    );
#endif
            serviceProvider?.GetRequiredService<LingoSdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}
