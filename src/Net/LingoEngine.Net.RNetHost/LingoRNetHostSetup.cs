using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Extension methods to configure the RNet host server.
/// </summary>
public static class LingoRNetHostSetup
{
    /// <summary>
    /// Registers and starts the host server used by the RNet tooling.
    /// </summary>
    /// <param name="reg">Engine registration.</param>
    /// <param name="port">Port on which the server will listen.</param>
    /// <returns>The same registration instance for chaining.</returns>
    public static ILingoEngineRegistration WithRNetHostServer(this ILingoEngineRegistration reg, int port = 61699)
    {
        reg.ServicesMain(s =>
        {
            s.AddSingleton<IRNetConfiguration>(new RNetConfiguration { Port = port });
            s.AddSingleton<IRNetServer, RNetServer>();
            s.AddSingleton<IRNetPublisher>(p => p.GetRequiredService<IRNetServer>().Publisher);
        });

        reg.AddPreBuildAction(p =>
        {
            var config = p.GetRequiredService<IRNetConfiguration>();
            if (config.AutoStartRNetHostOnStartup)
            {
                var server = p.GetRequiredService<IRNetServer>();
                server.StartAsync().GetAwaiter().GetResult();
            }
        });

        return reg;
    }
}
