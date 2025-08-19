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
            IServiceProvider? serviceProvider = null;
            services.RegisterLingoEngine(c => c
                    .WithLingoSdlEngine("TetriGrounds", 730, 547)
                    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject(sp => serviceProvider = sp)
                    );
            serviceProvider?.GetRequiredService<SdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}
