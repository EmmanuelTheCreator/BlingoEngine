using LingoEngine.SDL2;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.SDL2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.RegisterLingoEngine(c => c
                    .WithLingoSdlEngine("TetriGrounds", 640, 480)
                    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject()
                    );
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<SdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}
