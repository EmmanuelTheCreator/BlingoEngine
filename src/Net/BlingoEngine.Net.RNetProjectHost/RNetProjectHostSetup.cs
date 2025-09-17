using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Net.RNetProjectHost;

/// <summary>
/// Extension methods to configure the RNet host server.
/// </summary>
public static class BlingoRNetProjectHostSetup
{
    /// <summary>
    /// Registers and starts the host server used by the RNet tooling.
    /// </summary>
    /// <param name="reg">Engine registration.</param>
    /// <param name="port">Port on which the server will listen.</param>
    /// <param name="autoStart">Auto start at startup</param>
    /// <returns>The same registration instance for chaining.</returns>
    public static IBlingoEngineRegistration WithRNetProjectHostServer(this IBlingoEngineRegistration reg, int port = 61699, bool autoStart = false)
    {
        reg.ServicesMain(s => s
            .AddSingleton<IRNetConfiguration>(new RNetConfiguration { Port = port })
            .AddSingleton<IRNetProjectServer, RNetProjectServer>()
            .AddSingleton<IRNetPublisherEngineBridge, RNetProjectPublisher>()
            .AddSingleton<IRNetProjectBus, RNetProjectBus>()
            );

        reg.AddPostBuildAction(p =>
        {
            var config = p.GetRequiredService<IRNetConfiguration>();

            if (config.AutoStartRNetHostOnStartup || autoStart)
            {
                var server = p.GetRequiredService<IRNetProjectServer>();
                server.StartAsync().GetAwaiter().GetResult();
                var publisher = p.GetRequiredService<IRNetPublisherEngineBridge>();
                publisher.Enable(p.GetRequiredService<IBlingoPlayer>());
            }
        });

        return reg;
    }
}

